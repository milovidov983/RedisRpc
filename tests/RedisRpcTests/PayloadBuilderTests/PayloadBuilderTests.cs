using RedisRpc.Builders;
using RedisRpcTests.CommonExtentions;
using System.Linq;
using System.Text;
using Xunit;

namespace RedisRpcTests {
	public class PayloadBuilderTests {
		[Fact]
		public void SetOneStream_ReturneOneStream() {
			var testMessage = "Test message".ToStream();

			var payload = new PayloadBuilder()
				.WithStream(testMessage)
				.Build();

			Assert.Single(payload.Offsets);
		}

		[Fact]
		public void SetOneStream_ReturneOneProperStream() {
			var testMessage1 = "Test message 1";
			var testMessage2 = "Test message 2";


			var payload = new PayloadBuilder()
				.WithStream(testMessage1.ToStream())
				.WithStream(testMessage2.ToStream())
				.Build();

			var first = payload.Body.Take(payload.Offsets.First()).ToArray();
			var result = Encoding.UTF8.GetString(first);


			Assert.Equal(testMessage1, result);
		}

	}
}
