using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisRpc {
	public static class Extentions {
		public static JsonSerializerSettings JsonSettings { get; private set; }
		public static string ToJson<T>(T payload) {
			return JsonConvert.SerializeObject(payload, JsonSettings);
		}
	}
}
