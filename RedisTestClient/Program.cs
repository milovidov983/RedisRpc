using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RedisTestClient {
	class Program {

		private static string QueueName = "RedisTest:Handler:rpc";
		static async Task Main(string[] args) {
			Console.Title = "RedisTestClient";
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");

			IDatabase db = redis.GetDatabase();

			ISubscriber sub = redis.GetSubscriber();
			for (; ;){

				var value = await db.ListRightPushAsync(QueueName, "hello");
				sub.Publish(QueueName, "new message come", CommandFlags.FireAndForget);

				Console.WriteLine("message insert...");
				Console.ReadKey();

			}

			Console.WriteLine("exit");
		}
	}
}
