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
		public void SetTwoStream_ReturneTwoValidStream() {
			var firstTestData = "42";
			var secondTestData = "Test message 2";


			var payload = new PayloadBuilder()
				.WithStream(firstTestData.ToStream())
				.WithStream(secondTestData.ToStream())
				.Build();

			

			var first = payload.Body.Take(payload.Offsets[1]).ToArray();
			var second = payload.Body.Skip(payload.Offsets[1]).ToArray();

			var firstResult = Encoding.UTF8.GetString(first);
			var secondResult = Encoding.UTF8.GetString(second);


			Assert.Equal(firstTestData, firstResult);
			Assert.Equal(secondTestData, secondResult);
		}

	}
}
