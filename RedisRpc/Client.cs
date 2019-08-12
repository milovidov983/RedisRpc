namespace RedisRpc {
	using Newtonsoft.Json;
	using RedisRpc.Helpers;
	using RedisRpc.Interfaces;
	using RedisRpc.Models;
	using StackExchange.Redis;
	using System;
	using System.Collections.Concurrent;
	using System.Threading.Tasks;
    using System.Timers;

    internal class Client: IDisposable {
		// ? как удалять отсюда?
		private static readonly ConcurrentDictionary<Guid, ResponseMessage> awaitingRequests = new ConcurrentDictionary<Guid, ResponseMessage>();
		// ! нужна еще одна коллекция для таймаут лямбд которые будут абортить TCS?

		private readonly IDatabase database;
		private readonly ISubscriber subscriber;
		private readonly string responseTopic;
		private readonly TimeSpan timeout;
		private readonly ILogger logger;

		public Client(IConnection connection, ILogger logger) {
			this.database = connection.Database;
			this.subscriber = connection.Subscriber;
			this.responseTopic = connection.ResponseTopic;
			this.timeout = connection.Timeout;
			this.logger = logger;
		}

		//TODO: не забыть про TimeSpan? timeout = null
		public async Task<TResponse> GetResponse<TResponse, TRequest>(string topic, TRequest request, bool raiseException = true, TimeSpan? timeout = null) {
			var rawContent = request.ToJson();

			var payload = Payload.GetBuilder()
				.WithRawContent(rawContent)
				.Build();

			var dm = new DeliveredMessage(responseTopic, payload);

			var responseTask = Get(dm.CorrelationId, timeout);
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
		public Task<Payload> Get(Guid correlationId, TimeSpan? timeout) {
			// Подготовка объекта получающего ответ.
			// Создаем объект для ответа
			var msgResponse = new ResponseMessage(timeout ?? this.timeout);
			//// Задача которая будет вызвана при получении ответа
			//var tcs = new TaskCompletionSource<Payload>();
			//// Привязываем задачу к событию
			//msgResponse.AddedPayload += (payload) => { tcs.SetResult(payload); };
			//msgResponse.Tcs = tcs;

			// Кладем подготовленный объект в коллекцию ожидающих ответ.
			awaitingRequests.TryAdd(correlationId, msgResponse);
			//awaitingRequests[correlationId].AddedPayload += (payload) => { tcs.SetResult(payload); };

			return msgResponse.Tcs.Task;
		}

		/// <summary>
		/// Главный цикл запускает подписку на канал ответа ResponseTopic.
		/// </summary>
		public void StartMainLoop() {
			subscriber.Subscribe(responseTopic, async (channel, message) => {
				var msg = await database.ListRightPopAsync(responseTopic);

				while (msg.HasValue) {
					var dm = JsonConvert.DeserializeObject<DeliveredMessage>(msg);

					if (awaitingRequests.ContainsKey(dm.CorrelationId)) {
						awaitingRequests[dm.CorrelationId].Payload = dm.Payload;
						awaitingRequests.TryRemove(dm.CorrelationId, out _);
					} else {
						logger.Fatal("The received CorrelationId does not match any of the sent ones.");
						//TODO создать механизм очистки словаря от неактуальных значений которые отвалились по таймауту.
					}
					msg = await database.ListRightPopAsync(responseTopic);
				}
			});
			subscriber.Publish(responseTopic, string.Empty);
		}

		public void Dispose() {
			if (subscriber != null) {
				subscriber.UnsubscribeAll(CommandFlags.FireAndForget);
			}
		}


		#region inherit classes
		private class ResponseMessage {
			private static System.Timers.Timer timer;

			public ResponseMessage(TimeSpan timeout) {
				this.Tcs = new TaskCompletionSource<Payload>();
				this.AddedPayload += (payload) => {
					if(payload.Exception != null) {
						this.Tcs.SetException(new RedisHubException("Error in remote service. See inner exception.", payload.Exception));
					}
					this.Tcs.SetResult(payload);
				};


				SetTimer(timeout);
			}

			// ? Тип делегата который будет вызван когда сообщение доставлено из очереди.
			public delegate void Handler(Payload payload);
			// Событие, возникающее при добавлении нагрузки из очереди редиса
			public event Handler AddedPayload;
			public TaskCompletionSource<Payload> Tcs { get; }
			/// <summary>
			/// При присваивании данному свойству объекта, 
			/// будет вызвано событие добавления данных из очереди.
			/// </summary>
			public Payload Payload {
				set {
					AddedPayload(value);
				}
			}

			private void SetTimer(TimeSpan timeout) {
				timer = new Timer(timeout.TotalMilliseconds);
				timer.Elapsed += (Object source, ElapsedEventArgs e) => Abort();
				timer.AutoReset = true;
				timer.Enabled = true;
			}

			public void SetException(Exception e) {
				Tcs.SetException(e);
			}

			public void Abort() {
				Tcs.SetCanceled();
			}
		}
		#endregion
	}
}
