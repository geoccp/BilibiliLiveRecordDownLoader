using System.Net.Http;

namespace LiveRecordDownLoader.Shared.Interfaces
{
	public interface IHttpClient
	{
		HttpClient Client { get; set; }
	}
}
