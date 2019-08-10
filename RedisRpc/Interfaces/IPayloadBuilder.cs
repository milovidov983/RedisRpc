namespace RedisRpc.Interfaces {
	using RedisRpc.Models;
	using System.IO;
	internal interface IPayloadBuilder {
		IPayloadBuilder WithRawContent(string rawContent);
		IPayloadBuilder WithStream(Stream stream);
		IPayloadBuilder WithError(string error);
		IPayloadBuilder WithStatusCode(int statusCode);
		IPayloadBuilder WithExtendedInfo(string extendedInfo);
		Payload Build();
	}
}