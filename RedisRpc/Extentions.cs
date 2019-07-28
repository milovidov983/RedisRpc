using Newtonsoft.Json;
using RedisRpc.Models;

namespace RedisRpc {
	public static class Extentions {
		public static JsonSerializerSettings JsonSettings { get; private set; }
		public static string ToJson<T>(this T payload) {
			return JsonConvert.SerializeObject(payload, JsonSettings);
		}
		public static T GetContent<T>(this Payload messageInfo) {
			return JsonConvert.DeserializeObject<T>(messageInfo.RawContent);
		}
	}
}
