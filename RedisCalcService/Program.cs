using Newtonsoft.Json;
using RedisRpc;
using RedisRpc.Models;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

using RCC = RedisCalc.Contracts;

namespace RedisCalcService {
	class Program {
		private static string Topic;
		static void Main(string[] args) {

			Topic = RCC.Add.Topic;

			Console.Title = "RedisCalcService";


			Console.WriteLine("Подключение к redis...");
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
			IDatabase db = redis.GetDatabase();
			ISubscriber sub = redis.GetSubscriber();

			sub.Subscribe(Topic, async (channel, message) => {
				await AddCommand(db);
			});
			Console.WriteLine("Подключено.");

			sub.Publish(Topic, "start"); // bootstrap
			RunAwaiter();

			Console.WriteLine();
		}

		private static async Task AddCommand(IDatabase db) {
			var msg = await db.ListRightPopAsync(Topic);
			if (msg.HasValue) {
				Console.WriteLine($"Receive message {Topic}");

				var request = JsonConvert.DeserializeObject<DeliveredMessage>(msg);

				var data = JsonConvert.DeserializeObject<RCC.Add.Request>(request.Payload.RawContent);

				var result = new RCC.Add.Response {
					Results = data.A * 2
				};

				var payload = Payload.GetBuilder()
					.WithRawContent(result.ToJson())
					.Build();

				var dm = new DeliveredMessage(payload) {
					CorrelationId = request.CorrelationId,
					ResponseTopic = request.ResponseTopic
				};



				Hub.SendResponseAsync(dm);


				await AddCommand(db);
			}
		}

		public class Hub {
			private static ConnectionMultiplexer redis;
			private static IDatabase db;
			private static ISubscriber sub;

			static Hub() {
				redis = ConnectionMultiplexer.Connect("localhost");
				db = redis.GetDatabase();
				sub = redis.GetSubscriber();
			}


			public async static void SendResponseAsync(DeliveredMessage dm) {
				var value = await db.ListRightPushAsync(dm.ResponseTopic, dm.ToJson());
				sub.Publish(dm.ResponseTopic, "new message come", CommandFlags.FireAndForget);
			}
		}



		private static void RunAwaiter() {
			var exitEvent = new System.Threading.ManualResetEvent(false);
			Console.CancelKeyPress += (s, e) => {
				e.Cancel = true;
				exitEvent.Set();
			};
			exitEvent.WaitOne();
		}
	}
}