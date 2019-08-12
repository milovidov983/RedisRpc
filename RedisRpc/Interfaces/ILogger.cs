namespace RedisRpc.Interfaces {
	using System;
	using System.Collections.Generic;
	public interface ILogger {
		void Info(string message);
		void Info(Exception exeption, string message);
		void Info(Exception exeption, IDictionary<string, object> custom = null);

		void Debug(string message);
		void Debug(Exception exeption, string message);
		void Debug(Exception exeption, IDictionary<string, object> custom = null);

		void Error(string message);
		void Error(Exception exeption, string message);
		void Error(Exception exeption, IDictionary<string, object> custom = null);

		void Warn(string message);
		void Warn(Exception exeption, string message);
		void Warn(Exception exeption, IDictionary<string, object> custom = null);

		void Fatal(string message);
		void Fatal(Exception exeption, string message);
		void Fatal(Exception exeption, IDictionary<string, object> custom = null);
	}
}
