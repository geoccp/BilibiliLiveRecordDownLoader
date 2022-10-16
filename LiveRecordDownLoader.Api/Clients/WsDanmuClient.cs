using Api.Model.DanmuConf;
using Microsoft.Extensions.Logging;

namespace Api.Clients
{
	public class WsDanmuClient : WssDanmuClient
	{
		protected override string Server => $@"ws://{Host}:{Port}/sub";
		protected override ushort DefaultPort => 2244;

		public WsDanmuClient(ILogger<WsDanmuClient> logger, BilibiliApiClient apiClient) : base(logger, apiClient) { }

		protected override ushort GetPort(HostServerList server)
		{
			return server.ws_port;
		}
	}
}
