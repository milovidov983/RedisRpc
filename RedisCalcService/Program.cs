using Newtonsoft.Json;
using RedisRpc;
using RedisRpc.Models;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

using RCC = RedisCalc.Contracts;



namespace RedisCalcService {
	class Program {
		private static string listKey;
		static void Main(string[] args) {

			listKey = RCC.Add.Topic;

			Console.Title = "RedisCalc SERVER";


			Console.WriteLine("Подключение к redis...");
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
			IDatabase db = redis.GetDatabase();
			ISubscriber sub = redis.GetSubscriber();

			sub.Subscribe(listKey, async (channel, message) => {
				await AddCommand(db);
			});
			Console.WriteLine("Подключено.");

			sub.Publish(listKey, "start"); // bootstrap
			RunAwaiter();

			Console.WriteLine();
		}

		private static async Task AddCommand(IDatabase db) {
			// получение данных из списка редиса по адресу ключу
			// TODO ListRightPopAsync -> Queue 
			// я бы обернул этот метод ListRightPopAsync что бы тут было Dequeue, 
			// таким образом было бы понятно о каких таких очередях идет речь.


			var msg = await db.ListRightPopAsync(listKey);

			if (msg.HasValue) {
				// если данные есть то десериализуем нагрузку и выполняем над ними действие
				// затем пытаемся получить еще, если нет данных то выходим и ждём следующий момент,
				// когда кто-то из клиентов дёрнет за ручку сигнализирующую о том что в очереди появились сообщения.
				Console.WriteLine($"Receive message {listKey}");

				/// Десериализуем объект DeliveredMessage
				// ! продумать как обрабатывать доп поля
				var message = JsonConvert.DeserializeObject<DeliveredMessage>(msg);

				//!---------------------------------------------------------------------------------
				//executeImpl start here?

				/// Получения непосредственно полезной нагрузки.
				//var data = JsonConvert.DeserializeObject<RCC.Add.Request>(request.Payload.RawContent);
				var data = message.GetContent<RCC.Add.Request>(); 

				/// Какие то манипуляции с полученными данными.
				var result = new RCC.Add.Response {
					Results = data.A * 2
				};


				// отправка ответа
				Hub.SetRpcResultAsync(message, result);

				//executeImpl end here?
				//!---------------------------------------------------------------------------------
				


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

			public async static void SetRpcResultAsync<T>(DeliveredMessage dm, T payload, int? statusCode = null) {
				var result = Payload.GetBuilder()
					.WithRawContent(payload.ToJson())
					.WithStatusCode(statusCode ?? 200)
					.Build();

				var response = new DeliveredMessage(dm.ResponseKey, result);

				//Кладем результат работы микросервиса в лист ответа редиса.
				await db.ListRightPushAsync(response.ResponseKey, response.ToJson());
				//Оповещаем клиента по средством посылки сообщения в канал с одноименным с листом именем,
				//что результат положен в лист ответа и его можно забрать.
				sub.Publish(response.ResponseKey, "new message come", CommandFlags.FireAndForget);
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