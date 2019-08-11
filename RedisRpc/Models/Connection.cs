namespace RedisRpc.Models {
	using RedisRpc.Interfaces;
	using StackExchange.Redis;
	internal class Connection : IConnection {
		public Connection(IDatabase database, ISubscriber subscriber, string responseTopic) {
			Database = database;
			Subscriber = subscriber;
			ResponseTopic = responseTopic;
		}

		public IDatabase Database { get; }
		public ISubscriber Subscriber { get; }
		public string ResponseTopic { get; }


	}
}
