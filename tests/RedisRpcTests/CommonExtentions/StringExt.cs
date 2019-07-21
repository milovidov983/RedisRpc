using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RedisRpcTests.CommonExtentions {
	public static class StringExt {
		public static Stream ToStream(this string str) {
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(str);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
}
