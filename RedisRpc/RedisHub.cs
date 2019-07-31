using Newtonsoft.Json;
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
				throw new Exception("Error creating RedisHub instance. See inner exception.", e);
			}
		}

		private static ConfigurationOptions CreateConfiguration(Options options) {
			if (options == null) {
				throw new ArgumentNullException($"It is necessary to set the settings, create the configuration class {nameof(Options)} and initialize {nameof(RedisHub)} with it.");
			}
			var redisConfig = options?.RedisConfigurationOptions?.Clone() ?? new ConfigurationOptions();
			redisConfig.AddHosts(options.HostCollection);
			return redisConfig;
		}

		public void Dispose() {
			if (subscriber != null) {
				subscriber.UnsubscribeAll(CommandFlags.FireAndForget);
			}
		}


		/// <summary>
		/// топик для ответа.
		/// TODO предусмотреть как удалять зависшие очереди когда консьюмер ушёл а сервис ему ответил или
		/// когда он не успел обработать все ответы.
		/// Наверное это можно сделать средаствами настройки редиса,
		/// но мне хотелось бы что бы библиотека работала с дефолтным редисом без проблем.
		/// 
		/// 1) Проверять настройки редиса и если не устаовлен таймаут для очередей то ставить его.
		/// OR
		/// 2) Держать специальный список на редисе с (response__key, updatedAt)
		/// и что бы каждый сам себя обновлял и смотрел если есть просроченные то удалял бы их.
		/// </summary>
		public static string responceTopic = $"response__{Guid.NewGuid()}";
		public async Task<TResponse> GetResponse<TResponse, TRequest>(string topic, TRequest request, bool raiseException = true, TimeSpan? timeout = null) {
			var rawContent = request.ToJson();

			var payload = Payload.GetBuilder()
				.WithRawContent(rawContent)
				.Build();


			var dm = new DeliveredMessage(payload) {
				ResponseTopic = responceTopic,
				CorrelationId = Guid.NewGuid()
			};


			var responseTask = Hub.Get(dm.CorrelationId);
			var value = await db.ListRightPushAsync(topic, dm.ToJson());
			sub.Publish(topic, "new message come", CommandFlags.FireAndForget);

			var response = await responseTask;

			if (string.IsNullOrEmpty(response.Error)) {
				return JsonConvert.DeserializeObject<TResponse>(response.RawContent);
			} else {
				throw new Exception(response.Error);
			}


		}


		public static class Hub {
			private static string QueueName = "RedisTest:Handler:rpc";
			private static readonly ConcurrentDictionary<Guid, Message> rpcRequests = new ConcurrentDictionary<Guid, Message>();
			public static bool Running;

			public class Message {
				public delegate void Handler(Payload p);
				// Событие, возникающее при добавлении нагрузки из очереди редиса
				public event Handler AddedPayload;
				public Payload Payload {
					set {
						AddedPayload(value);
					}
				}
			}


			public static Task<Payload> Get(Guid correlationId) {
				var msgResponse = new Message();

				rpcRequests.TryAdd(correlationId, msgResponse);
				var tcs = new TaskCompletionSource<Payload>();
				rpcRequests[correlationId].AddedPayload += (payload) => { tcs.SetResult(payload); };

				return tcs.Task;
			}

			public static ISubscriber StartMainLoop() {
				ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
				IDatabase db = redis.GetDatabase();
				ISubscriber sub = redis.GetSubscriber();

				sub.Subscribe(responceTopic, async (channel, message) => {
					var msg = await db.ListRightPopAsync(responceTopic);

					while (msg.HasValue) {
						var dm = JsonConvert.DeserializeObject<DeliveredMessage>(msg.ToString());
						rpcRequests[dm.CorrelationId].Payload = dm.Payload;

						msg = await db.ListRightPopAsync(responceTopic);
					}
				});
				sub.Publish(QueueName, "bootstrap");
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
