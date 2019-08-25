namespace RedisRpc.Models {
	using System;
	public class DeliveredMessage {
		public Payload Payload { get; set; }
		public string ResponseKey { get; set; }
		public string Topic { get; set; }
		public Guid CorrelationId { get; set; } = Guid.NewGuid();

		public DeliveredMessage(string responseTopic, Payload payload, Guid? correlationId = null) {
			ResponseKey = responseTopic;
			CorrelationId = correlationId ?? Guid.NewGuid();
			Payload = payload;
		}

		public DeliveredMessage(string topic, string responseTopic, Payload payload, Guid? correlationId = null) {
			ResponseKey = responseTopic;
			CorrelationId = correlationId ?? Guid.NewGuid();
			Payload = payload;
			Topic = topic;
		}

		public DeliveredMessage() {
		}
	}
}
