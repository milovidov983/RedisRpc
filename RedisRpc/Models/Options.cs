using StackExchange.Redis;
using System.Collections.Generic;

namespace RedisRpc.Models {
	public class Options {
		public bool UseRedisChannelControl { get; set; }
		public List<(string Host, int Port)> HostCollection { get; set; }
		public ConfigurationOptions RedisConfigurationOptions { get; set; }

	}
}
