namespace RedisRpc.Models {
	using System;
	internal class DeliveredMessage {
		public Payload Payload { get; set; }
		public string ResponseKey { get; set; }
		public Guid CorrelationId { get; set; } = Guid.NewGuid();

		public DeliveredMessage(string responseTopic, Payload payload, Guid? correlationId = null) {
			ResponseKey = responseTopic;
			CorrelationId = correlationId ?? Guid.NewGuid();
			Payload = payload;
		}

		public DeliveredMessage() {
		}
	}
}
