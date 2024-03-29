﻿namespace RedisRpc {
	using Newtonsoft.Json;
	using RedisRpc.Models;
	using StackExchange.Redis;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	internal static class Extentions {
		public static JsonSerializerSettings JsonSettings { get; private set; }
		public static string ToJson<T>(this T payload) {
			return JsonConvert.SerializeObject(payload, JsonSettings);
		}
		public static T GetContent<T>(this DeliveredMessage messageInfo) { 
			return JsonConvert.DeserializeObject<T>(messageInfo.Payload.RawContent);
		}

		public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
		
		

		#region Configuration helpers
		public static void AddHosts(this ConfigurationOptions configurationOptions, List<(string Host, int Port)> hosts) {
			if(hosts?.Any() != true) {
				throw new ArgumentException($"You must specify at least one host and port.");
			}

			foreach(var item in hosts) {
				configurationOptions.EndPoints.Add(item.Host, item.Port);
			}
		}
		#endregion
	}
}
