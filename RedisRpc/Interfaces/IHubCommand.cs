using RedisRpc.Models;
using System.Threading.Tasks;

namespace RedisRpc.Interfaces {
	public interface IHubCommand {
		Task Execute(Payload dm);
	}
}
