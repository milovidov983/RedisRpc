using System;
using System.Linq;

namespace PerformanceCheck {
	class Program {
		private static byte[][] binaries;

		private const int arrayCount = 100;
		private const int arraysSize = 10000;
		private const int iterations = 100;


		static void Main(string[] args) {
			binaries = Enumerable
				.Range(0, arrayCount)
				.Select(x => {

					var randData = Enumerable
									.Range(0, arraysSize)
									.Select(y => (byte)y)
									.ToArray();

					return randData;

				}).ToArray();


			var watch = System.Diagnostics.Stopwatch.StartNew();

			foreach(var _ in Enumerable.Repeat(1, iterations)) {
				TestBufferBlock();
			}

			watch.Stop();
			var elapsedMs = watch.ElapsedMilliseconds;
			//using (StreamWriter sw = new StreamWriter("Buffer.BlockCopy.txt", true, System.Text.Encoding.Default)) {
			//	sw.WriteLine($"array count: {arrayCount}");
			//	sw.WriteLine($"arrays size: {arraysSize}");
			//	sw.WriteLine($"iterations: {iterations}");
			//	sw.WriteLine($"elapsed ms: {elapsedMs}");
			//}
			Console.WriteLine($"array count: {arrayCount}");
			Console.WriteLine($"arrays size: {arraysSize}");
			Console.WriteLine($"iterations: {iterations}");
			Console.WriteLine($"elapsed ms: {elapsedMs}");

		}

		private static void TestBufferBlock() {
			byte[] body = new byte[binaries.Sum(b => b.Length)];
			var length = 0;
			foreach (byte[] binary in binaries) {
				Buffer.BlockCopy(binary, 0, body, length, binary.Length);
				length += binary.Length;
				
			}
		}


	}
}
