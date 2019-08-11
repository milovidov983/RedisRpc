namespace RedisRpc.Models {
	using StackExchange.Redis;
	using System;
	using System.Collections.Generic;
	public class Options {
		// ? что я имел ввиду?
		public bool UseRedisChannelControl { get; set; }
		//TODO отрефакторить, использовать стандартные настройки редиса тк эта коллекция излишество.
		public List<(string Host, int Port)> HostsCollection { get; set; }
		public ConfigurationOptions RedisConfigurationOptions { get; set; }
		public TimeSpan Timeout { get; set; } = new TimeSpan(0, 0, 20);
	}
}