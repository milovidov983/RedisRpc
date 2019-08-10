﻿namespace RedisRpc.Models {
	using RedisRpc.Builders;
	using RedisRpc.Interfaces;
	using System;
	internal class Payload {
		public readonly string RawContent;
		public readonly string Error;
		public readonly string ExtendedInfo;
		public readonly byte[] Body;
		public readonly int? StatusCode;

		internal Payload(
		  string rawContent,
		  byte[] body,
		  string error,
		  int? statusCode,
		  string extendedInfo) {
			RawContent = rawContent;
			Body = body ?? Array.Empty<byte>();
			Error = error;
			ExtendedInfo = extendedInfo;
			StatusCode = statusCode;
		}

		internal static IPayloadBuilder GetBuilder() {
			return new PayloadBuilder();
		}
	}
}