using RedisRpc.Builders;
using RedisRpcTests.CommonExtentions;
using System.Linq;
using System.Text;
using Xunit;

namespace RedisRpcTests {
	public class PayloadBuilderTests {
		[Fact]
		public void SetOneStream_ReturneOneValidStream() {

			var testMessage = "42";

			var payload = new PayloadBuilder()
				.WithStream(testMessage.ToStream())
				.Build();


			var result = Encoding.UTF8.GetString(payload.Body);

			Assert.Equal(testMessage, result);
		}


	}
}
