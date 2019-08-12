namespace RedisRpc.Models {
	using RedisRpc.Builders;
	using RedisRpc.Interfaces;
	using System;
	internal class Payload {
		public string RawContent;
		public string Error;
		public string ExtendedInfo;
		public byte[] Body;
		public int? StatusCode;
		public Exception Exception;

		internal Payload(
		  string rawContent,
		  byte[] body,
		  string error,
		  int? statusCode,
		  string extendedInfo,
		  Exception exception) {
			RawContent = rawContent;
			Body = body ?? Array.Empty<byte>();
			Error = error;
			ExtendedInfo = extendedInfo;
			StatusCode = statusCode;
			Exception = exception;
		}

		internal Payload() { }

		internal static IPayloadBuilder GetBuilder() {
			return new PayloadBuilder();
		}
	}
}