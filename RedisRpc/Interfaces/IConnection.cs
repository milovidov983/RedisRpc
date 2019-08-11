namespace RedisRpc.Interfaces {
	using StackExchange.Redis;
    using System;

    internal interface IConnection {
		IDatabase Database { get; }
		ISubscriber Subscriber { get; }
		TimeSpan Timeout { get; }
		string ResponseTopic { get; }
	}
}
