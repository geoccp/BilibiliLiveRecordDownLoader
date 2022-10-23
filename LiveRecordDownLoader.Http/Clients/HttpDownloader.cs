using LiveRecordDownLoader.Http.Interfaces;
using LiveRecordDownLoader.Shared.Abstractions;
using LiveRecordDownLoader.Shared.Interfaces;
using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LiveRecordDownLoader.Http.Clients
{
	public class HttpDownloader : ProgressBase, IDownloader, IHttpClient
	{
		public Uri? Target { get; set; }

		public string? OutFileName { get; set; }

		public HttpClient Client { get; set; }
		private Stream? _netStream;

		public HttpDownloader(HttpClient client)
		{
			Client = client;
		}

		public async Task GetStreamAsync(CancellationToken token)
		{
			_netStream = await Client.GetStreamAsync(Target, token);
		}

		public void CloseStream()
		{
			_netStream?.Dispose();
		}

		public async ValueTask DownloadAsync(CancellationToken token)
		{
			if (OutFileName is null or @"")
			{
				throw new ArgumentNullException(nameof(OutFileName));
			}

			_netStream ??= await Client.GetStreamAsync(Target, token);
			EnsureDirectory(OutFileName);
			await using var fs = new FileStream(OutFileName, FileMode.Create, FileAccess.Write, FileShare.Read);

			using (CreateSpeedMonitor())
			{
				await CopyToWithProgressAsync(_netStream, fs, token);
			}
		}
		/// <summary>
		/// 根据token，读取对应的字节流，并写入对应文件
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="token"></param>
		/// <param name="bufferSize"></param>
		/// <returns></returns>
		private async Task CopyToWithProgressAsync(Stream from, Stream to, CancellationToken token, int bufferSize = 81920)
		{
			using var memory = MemoryPool<byte>.Shared.Rent(bufferSize);
			while (true)
			{
				//从当前流中异步读取一系列字节，将流中的位置提前读取的字节数，并监视取消请求。
				var length = await from.ReadAsync(memory.Memory, token);
				if (length != 0)
				{
					//异步地将字节序列写入当前流，将此流中的当前位置提前写入的字节数，并监视取消请求。
					await to.WriteAsync(memory.Memory.Slice(0, length), token);
					ReportProgress(length);
				}
				else
				{
					break;
				}
			}
		}

		private void ReportProgress(long length)
		{
			Interlocked.Add(ref Last, length);
		}

		private static void EnsureDirectory(string path)
		{
			var dir = Path.GetDirectoryName(path);
			Directory.CreateDirectory(dir!);
		}
	}
}
