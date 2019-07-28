using System;
using System.Collections.Generic;
using System.Text;

namespace RedisRpc.Models {
	public class DeliveredMessage {
		public Payload Payload { get; set; }
		public string ResponseTopic { get; set; }
		public Guid CorrelationId { get; set; }

		public DeliveredMessage(Payload payload) {
			Payload = payload;

		}
	}
}
