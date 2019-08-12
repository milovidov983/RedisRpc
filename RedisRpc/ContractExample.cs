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
	}
}


//namespace ExampleApp {
//	using RedisRpc;
//    using RedisRpc.Interfaces;
//    using SSC = SomeService.Contracts;

//	public class Class1 {
//		IRedisHub rpcService;

//		public async Task UsageExample() {
//			var result = await rpcService.GetResponse<SSC.GetSomething.Response, SSC.GetSomething.Request>(
//						SSC.GetSomething.Topic,
//						new SSC.GetSomething.Request {
//							Id = 12345
//						});


			

//		}
//	}
//}
