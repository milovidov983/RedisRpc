using System;
using System.Collections.Generic;
using System.Text;

namespace RedisRpc.Builders {
	public class SubscriptionsBuilder {
		//public static IHubHandlersConfig DefineSubscribers(this IRabbitHub hub, int prefetchCount = 32);
		//public static IQueueHandlersConfig OnTopic(this IQueueHandlersConfig config, string topic, Func<DeliveredMessage, Task<MessageProcessResult>> handler);
		//public static IQueueHandlersConfig OnUnexpectedTopic(this IQueueHandlersConfig config, Func<DeliveredMessage, Task<MessageProcessResult>> handler);

		//public interface IHubHandlersConfig {
		//	IHubHandlersConfig ForQueue(string queue, Func<IQueueHandlersConfig, IQueueHandlersConfig> builder);
		//	Task<ISubscription> Start();
		//}
		//public interface IQueueHandlersConfig {
		//	IQueueHandlersConfig AfterExecute(Func<DeliveredMessage, MessageProcessResult, MessageProcessResult> handler);
		//	IQueueHandlersConfig BeforeExecute(Func<DeliveredMessage, bool> handler);
		//	IQueueHandlersConfig OnAuthorizationIssues(Func<VerificationResult, Permission, DeliveredMessage, bool> handler, SignatureType minimumSignatureRequirement = SignatureType.Digest);
		//	IQueueHandlersConfig OnException(Func<Exception, DeliveredMessage, bool> handler);
		//	IQueueHandlersConfig OnTopic(string topic, IRabbitCommand command);
		//	IQueueHandlersConfig OnUnexpectedTopic(IRabbitCommand command);
		//}
		
	}
}
