using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RedisTest {



	class Program {
		private static string QueueName = "RedisTest:Handler:rpc";
		static void Main(string[] args) {
			Console.Title = "RedisTestService";
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");

			IDatabase db = redis.GetDatabase();

			ISubscriber sub = redis.GetSubscriber();

			sub.Subscribe(QueueName, async (channel, message) => {
				await Handler(db);
			});
			sub.Publish(QueueName, "start"); // bootstrap
			RunAwaiter();

			Console.WriteLine();
		}

		/*
		 * 
		 * Надо написать вокруг этого обёртку посылать наверное байты
		 * предварительно сжимая(?) их стандартной функцие сжатия.
		 * 
		 * Флаги ошибок и доп нагрузка с телеметрией(?) и данными, 
		 * например информациией об эксепшене.
		 * 
		 * (?) опционально
		 */
		private static async Task Handler(IDatabase db) {
			var mesg = await db.ListRightPopAsync(QueueName);
			if (mesg.HasValue) {
				Console.WriteLine(mesg.ToString());
				await Handler(db);
			}
		}

		private static void RunAwaiter(){
			var exitEvent = new System.Threading.ManualResetEvent(false);
			Console.CancelKeyPress += (s, e) => {
				e.Cancel = true;
				exitEvent.Set();
			};
			exitEvent.WaitOne();
		}
	}
}
