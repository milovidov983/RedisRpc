using RedisRpc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RedisRpc.Interfaces {
	public interface IPayloadBuilder {
		IPayloadBuilder WithRawContent(string rawContent);

		IPayloadBuilder WithStream(Stream stream);

		IPayloadBuilder WithError(string error);

		IPayloadBuilder WithStatusCode(int statusCode);

		IPayloadBuilder WithExtendedInfo(string extendedInfo);

		Payload Build();
	}


}
