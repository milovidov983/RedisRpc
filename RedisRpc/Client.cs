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
		/// </summary>
		/// <param name="correlationId">
		/// Unique id of the response.
		/// </param>
		/// <returns>
		/// 
		/// </returns>
		public Task<Payload> Get(Guid correlationId, TimeSpan? timeout) {
			var msgResponse = new ResponseMessage(timeout ?? this.timeout);
			// Кладем подготовленный объект в коллекцию ожидающих ответ.
			awaitingRequests.TryAdd(correlationId, msgResponse);

			return msgResponse.Tcs.Task;
		}

		/// <summary>
		/// The main loop starts subscribing to the ResponseTopic response channel.
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
						// (функция Abort() в ResponseMessage)
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

		#region private class
		/// <summary>
		/// ResponseMessage encapsulates the logic for working with 
		/// the asynchronous operation of receiving messages from the Redis list.
		/// </summary>
		private class ResponseMessage {
			private static System.Timers.Timer timer;

			/// <summary>
			/// The type of delegate that will be called when the message is delivered from the Redis list.
			/// </summary>
			/// <param name="payload">
			/// Object received from a remote service.
			/// </param>
			public delegate void Handler(Payload payload);

			/// <summary>
			/// Event that occurs when a load is added from the Redis list
			/// </summary>
			public event Handler AddedPayload;

			/// <summary>
			/// A task that ends when a message from a remote service arrives from the Redis list.
			/// </summary>
			public TaskCompletionSource<Payload> Tcs { get; }

			/// <summary>
			/// It is expected that this property will be assigned an object
			/// that came from a remote service and received through the Redis list.
			/// When assigning an object to this property, 
			/// an event that completes the wait task will be raised.
			/// </summary>
			public Payload Payload {
				set {
					AddedPayload(value);
				}
			}

			/// <summary>
			/// ResponseMessage encapsulates the logic for working with 
			/// the asynchronous operation of receiving messages from the Redis list.
			/// </summary>
			/// <param name="timeout">
			/// Timeout after which the task is interrupted.
			/// </param>
			public ResponseMessage(TimeSpan timeout) {
				this.Tcs = new TaskCompletionSource<Payload>();
				this.AddedPayload += (payload) => {
					if (payload.Exception != null) {
						this.Tcs.SetException(
							new RedisHubException("Error in remote service. See inner exception.", payload.Exception)
							);
					}
					this.Tcs.SetResult(payload);
				};


				SetTimer(timeout);
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
