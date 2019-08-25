using RedisRpc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RedisRpc.Interfaces {
	public interface IRedisHub {
		Task<TResponse> GetResponse<TResponse, TRequest>(string topic, TRequest request, bool raiseException = true, TimeSpan? timeout = default(TimeSpan?));
		Task<Stream> GetResponseStream<TRequest>(string topic, TRequest request, bool raiseException = true, TimeSpan? timeout = default(TimeSpan?));
		Task<TResponse> WithStreamGetResponse<TResponse, TRequest>(string topic, TRequest request, Stream stream, bool raiseException = true, TimeSpan? timeout = default(TimeSpan?));
		Task SubscribeAsync(string queueName, Action<DeliveredMessage> onMessage, int prefetchCount = 32);
	}
}
