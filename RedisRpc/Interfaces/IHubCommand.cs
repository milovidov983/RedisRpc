namespace RedisRpc.Interfaces {
	using RedisRpc.Models;
	using System.Threading.Tasks;
	internal interface IHubCommand {
		Task Execute(Payload dm);
	}
}
