using System;
using System.IO;
using System.Threading.Tasks;

namespace SomeService.Contracts {
	using RedisRpc;
	/// <summary>
	/// GetSomething API descriptions.
	/// 
	/// Contract getting something from microservice.
	/// </summary>
	public class GetSomething {
		/// <summary>
		/// The name of the queue to which microservice subscribes.
		/// </summary>
		public const string Topic = "someService.getSomething.rpc";

		/// <summary>
		/// Microservice request contract.
		/// </summary>
		public class Request {
			/// <summary>
			/// Example optional field.
			/// </summary>
			public int Id { get; set; }
		}
		/// <summary>
		/// The contract of the answer from microservice.
		/// </summary>
		public class Response {
			/// <summary>
			/// Example optional field.
			/// </summary>
			public string Results { get; set; }
		}
		/// <summary>
		/// (later remake in struct)
		/// byte v1 = 1
		/// byte v2 = 0
		/// byte v3 = 1
		/// byte v4 = 0
		/// 1.0.1.0
		/// </summary>
		public const int Version = 1;
	}
}


namespace RedisRpc {
	public interface IRpcRedis {
		Task<TResponse> GetResponse<TResponse, TRequest>(string topic, TRequest request, bool raiseException = true, TimeSpan? timeout = default(TimeSpan?));
		Task<Stream> GetResponseStream<TRequest>(string topic, TRequest request, bool raiseException = true, TimeSpan? timeout = default(TimeSpan?));
		Task<TResponse> WithStreamGetResponse<TResponse, TRequest>(string topic, TRequest request, Stream stream, bool raiseException = true, TimeSpan? timeout = default(TimeSpan?));
	}
}

namespace ExampleApp {
	using RedisRpc;
	using SSC = SomeService.Contracts;

	public class Class1 {
		IRpcRedis rpcService;

		public async Task UsageExample() {
			var result = await rpcService.GetResponse<SSC.GetSomething.Response, SSC.GetSomething.Request>(
						SSC.GetSomething.Topic,
						new SSC.GetSomething.Request {
							Id = 12345
						});

		}
	}
}
