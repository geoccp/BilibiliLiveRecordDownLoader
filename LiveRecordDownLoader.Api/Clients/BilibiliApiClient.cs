using Api.Utils;
using LiveRecordDownLoader.Shared.Interfaces;
using LiveRecordDownLoader.Shared.Utils;
using CryptoBase;
using CryptoBase.Abstractions.Digests;
using CryptoBase.Digests.MD5;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Clients
{
	public partial class BilibiliApiClient : IHttpClient
	{
		//System.Net.Http 是微软.net4.5中推出的HTTP 应用程序的编程接口， 微软称之为“现代化的 HTTP 编程接口”， 主要提供如下内容：
		//用户通过 HTTP 使用现代化的 Web Service 的客户端组件；
		//能够同时在客户端与服务端同时使用的 HTTP 组件（比如处理 HTTP 标头和消息）， 为客户端和服务端提供一致的编程模型。
		public HttpClient Client { get; set; }
		//对可同时访问资源或资源池的线程数加以限制的 Semaphore 的轻量替代。
		private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);

		public BilibiliApiClient(HttpClient client)
		{
			Client = client;
		}
		//CancellationToken			传播有关应取消操作的通知。
		private async Task<T?> GetJsonAsync<T>(string url, CancellationToken token)
		{
			//WaitAsync(CancellationToken)	在观察 CancellationToken 时，输入 SemaphoreSlim 的异步等待。
			await SemaphoreSlim.WaitAsync(token);
			try
			{
				//GetFromJsonAsync(HttpClient, Uri, Type, CancellationToken)	
				//将 GET 请求发送到指定 URI，并在异步操作中以 JSON 形式返回反序列化响应正文生成的值。
				//url = "https://www.tiktok.com/@gusgml78907/live";
				return await Client.GetFromJsonAsync<T>(url, token);
			}
			finally
			{
				SemaphoreSlim.Release();
			}
		}

		private async Task<HttpResponseMessage> PostAsync(string url, HttpContent content, CancellationToken token)
		{
			await SemaphoreSlim.WaitAsync(token);
			try
			{
				return await Client.PostAsync(url, content, token);
			}
			finally
			{
				SemaphoreSlim.Release();
			}
		}

		private async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> pair, bool isSign, CancellationToken token)
		{
			//using  标识对应的对象，离开作用域后，会被垃圾回收
			using var content = await GetBodyAsync(pair, isSign);
			return await PostAsync(url, content, token);
		}

		private static async ValueTask<FormUrlEncodedContent> GetBodyAsync(Dictionary<string, string> pair, bool isSign)
		{
			if (isSign)
			{
				pair[@"appkey"] = AppConstants.AppKey;
				pair[@"ts"] = Timestamp.GetTimestamp(DateTime.UtcNow).ToString();
				pair = pair.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
				//使用应用程序/x-www-form-urlencoded MIME 类型编码的名称/值元组的容器。
				using var temp = new FormUrlEncodedContent(pair.Cast());
				//将 HTTP 内容序列化到字符串，此为异步操作。
				var str = await temp.ReadAsStringAsync();
				var md5 = Md5String(str + AppConstants.AppSecret);
				pair.Add(@"sign", md5);
			}

			return new FormUrlEncodedContent(pair.Cast());
		}

		private static string Md5String(in string str)
		{
			var buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(str.Length));
			try
			{
				var length = Encoding.UTF8.GetBytes(str, buffer);

				Span<byte> hash = stackalloc byte[HashConstants.Md5Length];

				MD5Utils.Default(buffer.AsSpan(0, length), hash);

				return hash.ToHex();
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}
	}
}
