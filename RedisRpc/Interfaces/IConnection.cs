namespace RedisRpc.Interfaces {
	using StackExchange.Redis;
	internal interface IConnection {
		IDatabase Database { get; }
		ISubscriber Subscriber { get; }
		string ResponseTopic { get; }
	}
}
