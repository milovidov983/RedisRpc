//using RedisRpc.Models;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace RedisRpc.Interfaces {
//	public interface IQueueHandlersConfig {
//		IQueueHandlersConfig AfterExecute(Func<Payload, MessageProcessResult, MessageProcessResult> handler);
//		IQueueHandlersConfig BeforeExecute(Func<Payload, bool> handler);
//		IQueueHandlersConfig OnException(Func<Exception, Payload, bool> handler);
//		IQueueHandlersConfig OnTopic(string topic, IRabbitCommand command);
//		IQueueHandlersConfig OnUnexpectedTopic(IRabbitCommand command);
//	}
//}
