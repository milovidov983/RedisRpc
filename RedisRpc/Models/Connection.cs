namespace RedisRpc.Models {
	using System;
	using RedisRpc.Interfaces;
	using StackExchange.Redis;
	internal class Connection : IConnection {
		public Connection(IDatabase database, ISubscriber subscriber, string responseTopic, TimeSpan timeout) {
			Database = database;
			Subscriber = subscriber;
			ResponseTopic = responseTopic;
			Timeout = timeout;
		}

		public IDatabase Database { get; }
		public ISubscriber Subscriber { get; }
		public string ResponseTopic { get; }
		public TimeSpan Timeout { get; }
	}
}
