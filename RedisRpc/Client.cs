namespace RedisRpc {
	using Newtonsoft.Json;
	using RedisRpc.Helpers;
	using RedisRpc.Interfaces;
	using RedisRpc.Models;
	using StackExchange.Redis;
	using System;
	using System.Collections.Concurrent;
	using System.Threading.Tasks;

	internal class Client: IDisposable {
		private static readonly ConcurrentDictionary<Guid, ResponseMessage> awaitingRequests = new ConcurrentDictionary<Guid, ResponseMessage>();
		private readonly IDatabase database;
		private readonly ISubscriber subscriber;
		private readonly string responseTopic;

		public Client(IConnection connection) {
			this.database = connection.Database;
			this.subscriber = connection.Subscriber;
			this.responseTopic = connection.ResponseTopic;
		}

		//TODO: не забыть про TimeSpan? timeout = null
		public async Task<TResponse> GetResponse<TResponse, TRequest>(string topic, TRequest request, bool raiseException = true, TimeSpan? timeout = null) {
			var rawContent = request.ToJson();

			var payload = Payload.GetBuilder()
				.WithRawContent(rawContent)
				.Build();

			var dm = new DeliveredMessage(responseTopic, payload);

			var responseTask = Get(dm.CorrelationId);
			await database.ListRightPushAsync(topic, dm.ToJson());
			subscriber.Publish(topic, "new message come", CommandFlags.FireAndForget);

			var response = await responseTask;

			if (string.IsNullOrEmpty(response.Error)) {
				return JsonConvert.DeserializeObject<TResponse>(response.RawContent);
			} else {
				if (raiseException) {
					throw new RedisHubException(response.Error);
				}
			}
			return default;
		}

		/// <summary>
		/// Функция создает объект ResponseMessage который помещает его в коллекцию ожидания(?)
		/// 
		/// </summary>
		/// <param name="correlationId">Уникальный id ответа.</param>
		/// <returns></returns>
		public static Task<Payload> Get(Guid correlationId) {
			// Подготовка объекта получающего ответ.
			// Создаем объект для ответа
			var msgResponse = new ResponseMessage();
			// Задача которая будет вызвана при получении ответа
			var tcs = new TaskCompletionSource<Payload>();
			// Привязываем задачу к событию
			msgResponse.AddedPayload += (payload) => { tcs.SetResult(payload); };

			// Кладем подготовленный объект в коллекцию ожидающих ответ.
			awaitingRequests.TryAdd(correlationId, msgResponse);
			//awaitingRequests[correlationId].AddedPayload += (payload) => { tcs.SetResult(payload); };

			return tcs.Task;
		}

		/// <summary>
		/// Главный цикл запускает подписку на канал ответа ResponseTopic.
		/// </summary>
		public void StartMainLoop() {
			subscriber.Subscribe(responseTopic, async (channel, message) => {
				var msg = await database.ListRightPopAsync(responseTopic);

				while (msg.HasValue) {
					var dm = JsonConvert.DeserializeObject<DeliveredMessage>(msg.ToString());
					awaitingRequests[dm.CorrelationId].Payload = dm.Payload;

					msg = await database.ListRightPopAsync(responseTopic);
				}
			});
			subscriber.Publish(responseTopic, string.Empty);
		}

		public void Dispose() {
			if (subscriber != null) {
				subscriber.UnsubscribeAll(CommandFlags.FireAndForget);
			}
			if(database != null) {
				
			}
		}


		#region inherit classes
		private class ResponseMessage {
			// ? Тип делегата который будет вызван когда сообщение доставлено из очереди.
			public delegate void Handler(Payload payload);
			// Событие, возникающее при добавлении нагрузки из очереди редиса
			public event Handler AddedPayload;

			/// <summary>
			/// При присваивании данному свойству объекта, 
			/// будет вызвано событие добавления данных из очереди.
			/// </summary>
			public Payload Payload {
				set {
					AddedPayload(value);
				}
			}
		}
		#endregion
	}
}
