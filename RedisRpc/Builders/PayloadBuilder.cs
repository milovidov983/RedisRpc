using RedisRpc.Interfaces;
using RedisRpc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RedisRpc.Builders {
	internal class PayloadBuilder : IPayloadBuilder {
		private string rawContent = string.Empty;
		private List<byte[]> binaries = new List<byte[]>();
		private string error;
		private string extendedInfo;
		private int? statusCode;

		public IPayloadBuilder WithRawContent(string rawContent) {
			this.rawContent = rawContent ?? string.Empty;
			return this;
		}

		public IPayloadBuilder WithBinary(byte[] binary) {
			binaries.Add(binary);
			return this;
		}

		public IPayloadBuilder WithStream(Stream stream) {
			var buffer = new byte[stream.Length];
			using (MemoryStream memoryStream = new MemoryStream(buffer)) {
				stream.CopyTo(memoryStream);
			}
			binaries.Add(buffer);
			return this;
		}

		public IPayloadBuilder WithError(string error) {
			this.error = error;
			return this;
		}

		public IPayloadBuilder WithExtendedInfo(string extendedInfo) {
			this.extendedInfo = extendedInfo;
			return this;
		}

		public IPayloadBuilder WithStatusCode(int statusCode) {
			this.statusCode = new int?(statusCode);
			return this;
		}

		public virtual Payload Build() {
			var bytes = Encoding.UTF8.GetBytes(rawContent);
			List<int> offsets = new List<int>();
			byte[] body;
			if (binaries.Any()) {
				var length = bytes.Length;
				body = new byte[length + binaries.Sum(b => b.Length)];
				Buffer.BlockCopy(bytes, 0, body, 0, length);
				foreach (byte[] binary in binaries) {
					offsets.Add(length);
					Buffer.BlockCopy(binary, 0, body, length, binary.Length);
					length += binary.Length;
				}
			} else {
				body = bytes;
			}
			return new Payload(rawContent, body, offsets.ToArray(), error, statusCode, extendedInfo);
		}
	}
}
