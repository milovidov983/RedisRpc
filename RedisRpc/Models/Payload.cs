using RedisRpc.Builders;
using RedisRpc.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisRpc.Models {
	public class Payload {
		public readonly string RawContent;
		public readonly string Error;
		public readonly string ExtendedInfo;
		public readonly byte[] Body;
		public readonly int[] Offsets;
		public readonly int? StatusCode;

		public Payload(
		  string rawContent,
		  byte[] body,
		  int[] offsets,
		  string error,
		  int? statusCode,
		  string extendedInfo) {
			RawContent = rawContent;
			Body = body ?? Array.Empty<byte>();
			Offsets = offsets ?? Array.Empty<int>();
			Error = error;
			ExtendedInfo = extendedInfo;
			StatusCode = statusCode;
		}

		public static IPayloadBuilder GetBuilder() {
			return new PayloadBuilder();
		}
	}
}