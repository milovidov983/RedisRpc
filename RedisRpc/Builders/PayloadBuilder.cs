﻿using RedisRpc.Interfaces;
using RedisRpc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RedisRpc.Builders {
	internal class PayloadBuilder : IPayloadBuilder {
		private string rawContent = string.Empty;
		private byte[] binary;
		private string error;
		private string extendedInfo;
		private int? statusCode;

		public IPayloadBuilder WithRawContent(string rawContent) {
			this.rawContent = rawContent ?? string.Empty;
			return this;
		}

		public IPayloadBuilder WithStream(Stream stream) {
			binary = new byte[stream.Length];
			using (MemoryStream memoryStream = new MemoryStream(binary)) {
				stream.CopyTo(memoryStream);
			}
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
			return new Payload(rawContent, binary, error, statusCode, extendedInfo);
		}
	}
}
