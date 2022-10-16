using LiveRecordDownLoader.Services;
using LiveRecordDownLoader.Utils;
using ModernWpf.Controls;
using Punchclock;
using System;
using System.Threading.Tasks;

#pragma warning disable VSTHRD001
namespace LiveRecordDownLoader.Views.Dialogs
{
	public class DisposableContentDialog : ContentDialog, IDisposable
	{
		private readonly OperationQueue _queue;

		public DisposableContentDialog()
		{
			_queue = DI.GetRequiredService<OperationQueue>();
			Owner = DI.GetRequiredService<MainWindow>();
		}

		public virtual void Dispose()
		{
			Hide();
		}

		public async Task<ContentDialogResult> SafeShowAsync(int priority = 1, ContentDialogResult defaultResult = ContentDialogResult.None)
		{
			return await _queue.Enqueue(priority, TaskQueueKeyConstants.ContentDialogKey, async () =>
			{
				var res = defaultResult;
				try
				{
					await Dispatcher.Invoke(async () =>
					{
						Owner!.Focus();
						res = await ShowAsync();
					});
				}
				catch (InvalidOperationException)
				{

				}
				return res;
			});
		}
	}
}
