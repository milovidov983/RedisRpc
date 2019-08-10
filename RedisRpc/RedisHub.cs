using Newtonsoft.Json;
using RedisRpc.Helpers;
using RedisRpc.Interfaces;
using RedisRpc.Models;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace RedisRpc {
	class RedisHub : IRedisHub, IDisposable {
		private static ISubscriber subscriber;
		private static IConnectionMultiplexer redis;
		private static IDatabase db;
		private static ISubscriber sub;
		private static Options options;

		public static void Setup(Action<Options> setupAction) {
			options = new Options();
			setupAction.Invoke(options);
		}

		public RedisHub(Options options) {
			try {
				var redisConfig = CreateConfiguration(options);

				redis = ConnectionMultiplexer.Connect(redisConfig);
				db = redis.GetDatabase();
				sub = redis.GetSubscriber();
				subscriber = Hub.StartMainLoop();
			} catch(Exception e) {
				throw new RedisHubException("Error creating RedisHub instance. See inner exception.", e);
			}
		}

		private static ConfigurationOptions CreateConfiguration(Options options) {
			if (options == null) {
				throw new ArgumentNullException($"It is necessary to set the settings, create the configuration class {nameof(Options)} and initialize {nameof(RedisHub)} with it.");
			}
			var redisConfig = options?.RedisConfigurationOptions?.Clone() ?? new ConfigurationOptions();
			redisConfig.AddHosts(options.HostsCollection);
			return redisConfig;
		}

		public void Dispose() {
			if (subscriber != null) {
				subscriber.UnsubscribeAll(CommandFlags.FireAndForget);
			}
		}

		/// <summary>
		/// Топик для ответа.
		/// </summary>
		/// <remarks>
		/// Ключ для redis list в который сервер кладёт ответы на запросы клиента. 
		/// И он же ключ для подписки на канал редиса который дёргается сервером когда он положил ответ в лист
		/// </remarks>
		/// TODO 
		/// Предусмотреть как удалять зависшие очереди(responseTopic) 
		/// когда консьюмер ушёл а сервис ему ответил в responseTopic 
		/// или когда он не успел обработать все ответы.
		/// Наверное это можно сделать средствами настройки редиса,
		/// но мне хотелось бы что бы библиотека работала с дефолтным редисом без проблем.
		/// 
		/// 1) Проверять настройки редиса и если не установлен тайм аут для очередей то ставить его.
		/// OR
		/// 2) Держать специальный список на редисе с (response__key, updatedAt)
		/// и что бы каждый сам себя обновлял и смотрел если есть просроченные то удалял бы их
		/// (чревато ошибками если не аккуратно написать систему по управлению этим велосипедом).
		public static string responseTopic = $"response__{Guid.NewGuid()}";
		
		//TODO: не забыть про TimeSpan? timeout = null
		public async Task<TResponse> GetResponse<TResponse, TRequest>(string topic, TRequest request, bool raiseException = true, TimeSpan? timeout = null) {
			var rawContent = request.ToJson();

			var payload = Payload.GetBuilder()
				.WithRawContent(rawContent)
				.Build();

			var dm = new DeliveredMessage(responseTopic, payload);

			var responseTask = Hub.Get(dm.CorrelationId);
			await db.ListRightPushAsync(topic, dm.ToJson());
			sub.Publish(topic, "new message come", CommandFlags.FireAndForget);

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


		private static class Hub {

			/// <summary>
			/// Произвольное имя ключа для запуска подписчика.
			/// </summary>
			private static string StartupRandomTopic = $"StartupRandomTopic:{Guid.NewGuid()}:rpc";
			/// <summary>
			// ? Список сообщений ожидающих ответа?
			/// </summary>
			private static readonly ConcurrentDictionary<Guid, ResponseMessage> awaitingRequests = new ConcurrentDictionary<Guid, ResponseMessage>();
			public static bool Running;

	
			public class ResponseMessage {				
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

			public static ISubscriber StartMainLoop() {
				ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
				IDatabase db = redis.GetDatabase();
				ISubscriber sub = redis.GetSubscriber();

				sub.Subscribe(responseTopic, async (channel, message) => {
					var msg = await db.ListRightPopAsync(responseTopic);

					while (msg.HasValue) {
						var dm = JsonConvert.DeserializeObject<DeliveredMessage>(msg.ToString());
						awaitingRequests[dm.CorrelationId].Payload = dm.Payload;

						msg = await db.ListRightPopAsync(responseTopic);
					}
				});
				sub.Publish(StartupRandomTopic, string.Empty);
				return sub;
			}
		}


		#region other methods
		public async Task<Stream> GetResponseStream<TRequest>(string topic, TRequest request, bool raiseException = true, TimeSpan? timeout = null) {
			throw new NotImplementedException();
		}

		public async Task<TResponse> WithStreamGetResponse<TResponse, TRequest>(string topic, TRequest request, Stream stream, bool raiseException = true, TimeSpan? timeout = null) {
			throw new NotImplementedException();
		}


		#endregion
	}
}
