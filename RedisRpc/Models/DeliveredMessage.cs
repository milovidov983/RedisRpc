namespace RedisRpc.Models {
	using System;
	internal class DeliveredMessage {
		public Payload Payload { get; }
		public string ResponseTopic { get;  }
		public Guid CorrelationId { get;  }

		public DeliveredMessage(string responseTopic, Payload payload) {
			Payload = payload;
			CorrelationId = Guid.NewGuid();
			ResponseTopic = responseTopic;
		}
	}
}
