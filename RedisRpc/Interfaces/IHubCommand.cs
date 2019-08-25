namespace RedisRpc.Interfaces {
	using RedisRpc.Models;
	using System.Threading.Tasks;
	public interface IHubCommand {
		Task Execute(DeliveredMessage dm);
	}
}