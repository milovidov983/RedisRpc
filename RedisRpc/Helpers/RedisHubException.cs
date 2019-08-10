namespace RedisRpc.Helpers {
	using System;
	/// <summary>
	/// RedisHub library exception class.
	/// </summary>
	public class RedisHubException : Exception {
		public RedisHubException(string message, Exception innerException) : base(message, innerException) {
		}
		public RedisHubException(string message) : base(message) {
		}
	}
}
