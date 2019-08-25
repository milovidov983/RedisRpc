using RedisRpc.Interfaces;
using RedisRpc.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedisRpc.Builders {
	public class SubscriptionsBuilder {
		public static IHubHandlersConfig DefineSubscribers(this IRedisHub hub, int prefetchCount = 32) {
			
		}

		public interface IHubHandlersConfig {
			IHubHandlersConfig ForQueue(string queue, Func<IQueueHandlersConfig, IQueueHandlersConfig> builder);
			Task Start();
		}
		public interface IQueueHandlersConfig {
			IQueueHandlersConfig AfterExecute(Action<DeliveredMessage> handler);
			IQueueHandlersConfig BeforeExecute(Action<DeliveredMessage, bool> handler);
			IQueueHandlersConfig OnException(Action<Exception, DeliveredMessage, bool> handler);
			IQueueHandlersConfig OnTopic(string topic, IHubCommand command);
			IQueueHandlersConfig OnUnexpectedTopic(IHubCommand command);
		}

		class QueueHandlersConfigBuilder : IQueueHandlersConfig {
			public Dictionary<string, IHubCommand> commands = new Dictionary<string, IHubCommand>();

			public IQueueHandlersConfig OnTopic(string topic, IHubCommand command) {
				commands.Add(topic, command);
				return this;
			}
			#region other
			public IQueueHandlersConfig AfterExecute(Action<DeliveredMessage> handler) {
				throw new NotImplementedException();
			}

			public IQueueHandlersConfig BeforeExecute(Action<DeliveredMessage, bool> handler) {
				throw new NotImplementedException();
			}

			public IQueueHandlersConfig OnException(Action<Exception, DeliveredMessage, bool> handler) {
				throw new NotImplementedException();
			}

			public IQueueHandlersConfig OnUnexpectedTopic(IHubCommand command) {
				throw new NotImplementedException();
			}
			#endregion
		}
		class Hub : IHubHandlersConfig {
			private IQueueHandlersConfig queueHandlersConfig;
			private string queue;
			public IHubHandlersConfig ForQueue(string queue, Func<IQueueHandlersConfig, IQueueHandlersConfig> builder) {
				this.queue = queue;
				queueHandlersConfig = builder(new QueueHandlersConfigBuilder());
				return this;
			}

			public Task Start() {
				throw new NotImplementedException();
			}
		}
	}
}
