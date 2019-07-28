using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisCalc.Contracts
{
    public class Add
    {
		/// <summary>
		/// The name of the queue to which microservice subscribes.
		/// </summary>
		public const string Topic = "redisCalcService.add.rpc";

		/// <summary>
		/// Microservice request contract.
		/// </summary>
		public class Request {
			/// <summary>
			/// Example optional field.
			/// </summary>
			public int A { get; set; }

			public int B { get; set; }
		}
		/// <summary>
		/// The contract of the answer from microservice.
		/// </summary>
		public class Response {
			/// <summary>
			/// Example optional field.
			/// </summary>
			public int Results { get; set; }
		}
	}
}
