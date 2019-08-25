using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RedisCalc.Service")]

namespace RedisRpc {
	using RedisRpc.Helpers;
	using RedisRpc.Interfaces;
	using RedisRpc.Models;
	using StackExchange.Redis;
	using System;
	using System.IO;
	using System.Threading.Tasks;
	public class RedisHub : IRedisHub, IDisposable {
		private static Options options;

		public static void Setup(Action<Options> setupAction) {
			options = new Options();
			setupAction.Invoke(options);
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
		private readonly string responseTopic = $"response__{Guid.NewGuid()}";
		private readonly ConnectionMultiplexer redis;
		private readonly Hub client;

		public RedisHub(Options options) {
			try {
				var redisConfig = CreateConfiguration(options);

				redis = ConnectionMultiplexer.Connect(redisConfig);

				var connection = new Connection(
					database: redis.GetDatabase(),
					subscriber: redis.GetSubscriber(),
					responseTopic: responseTopic,
					timeout: options.Timeout
					);

				client = new Hub(connection, options.Logger ?? new Logger());
				client.StartMainLoop();

			} catch(Exception e) {
				throw new RedisHubException("Error creating RedisHub instance. See inner exception.", e);
			}
		}

		private ConfigurationOptions CreateConfiguration(Options options) {
			if (options == null) {
				throw new ArgumentNullException($"It is necessary to set the settings, create the configuration class {nameof(Options)} and initialize {nameof(RedisHub)} with it.");
			}
			var redisConfig = options?.RedisConfigurationOptions?.Clone() ?? new ConfigurationOptions();
			redisConfig.AddHosts(options.HostsCollection);
			return redisConfig;
		}

		public async Task<TResponse> GetResponse<TResponse, TRequest>(string topic, TRequest request, bool raiseException = true, TimeSpan? timeout = null) {
			return await client.GetResponse<TResponse, TRequest>(topic, request, raiseException, timeout);
		}



		public Task SubscribeAsync(string queueName, Action<DeliveredMessage> onMessage, int prefetchCount = 32) {
			throw new NotImplementedException();
		}




		public void Dispose() {
			if(redis != null) {
				redis.Close();
			}
		}

		#region для получения стримов
		public async Task<Stream> GetResponseStream<TRequest>(string topic, TRequest request, bool raiseException = true, TimeSpan? timeout = null) {
			throw new NotImplementedException();
		}

		public async Task<TResponse> WithStreamGetResponse<TResponse, TRequest>(string topic, TRequest request, Stream stream, bool raiseException = true, TimeSpan? timeout = null) {
			throw new NotImplementedException();
		}

		#endregion
	}
}
