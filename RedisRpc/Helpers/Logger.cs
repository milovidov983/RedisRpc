namespace RedisRpc.Helpers {
	using RedisRpc.Interfaces;
	using System;
	using System.Collections.Generic;
	internal class Logger : ILogger {
		public Logger() {
		}

		public Logger(bool isEnable) {
			IsEnabled = isEnable;
		}

		public bool IsEnabled { get; set; } = true;

		public void Debug(string message) {
			if (IsEnabled) {
				Console.WriteLine(message);
			}
		}

		public void Debug(Exception exeption, string message) {
			if (IsEnabled) {
				Console.WriteLine(message);
			}
		}

		public void Debug(Exception exeption, IDictionary<string, object> custom = null) {
			if (IsEnabled) {
				Console.WriteLine(exeption.Message);
			}
		}

		public void Error(string message) {
			if (IsEnabled) {
				Console.WriteLine(message);
			}
		}

		public void Error(Exception exeption, string message) {
			if (IsEnabled) {
				Console.WriteLine(message);
			}
		}

		public void Error(Exception exeption, IDictionary<string, object> custom = null) {
			if (IsEnabled) {
				Console.WriteLine(exeption.Message);
			}
		}

		public void Fatal(string message) {
			if (IsEnabled) {
				Console.WriteLine(message);
			}
		}

		public void Fatal(Exception exeption, string message) {
			if (IsEnabled) {
				Console.WriteLine(message);
			}
		}

		public void Fatal(Exception exeption, IDictionary<string, object> custom = null) {
			if (IsEnabled) {
				Console.WriteLine(exeption.Message);
			}
		}

		public void Info(string message) {
			if (IsEnabled) {
				Console.WriteLine(message);
			}
		}

		public void Info(Exception exeption, string message) {
			if (IsEnabled) {
				Console.WriteLine(message);
			}
		}

		public void Info(Exception exeption, IDictionary<string, object> custom = null) {
			if (IsEnabled) {
				Console.WriteLine(exeption.Message);
			}
		}

		public void Warn(string message) {
			if (IsEnabled) {
				Console.WriteLine(message);
			}
		}

		public void Warn(Exception exeption, string message) {
			if (IsEnabled) {
				Console.WriteLine(message);
			}
		}

		public void Warn(Exception exeption, IDictionary<string, object> custom = null) {
			if (IsEnabled) {
				Console.WriteLine(exeption.Message);
			}
		}
	}
}
