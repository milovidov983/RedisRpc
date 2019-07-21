using System.IO;

namespace RedisRpcTests.CommonHelpers {
	public static class StringHelpers {
		public static Stream GenerateStream(string s) {
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
}
