using ReactiveUI;

namespace LiveRecordDownLoader.ViewModels
{
	public class LogViewModel : ReactiveObject, IRoutableViewModel
	{
		public string UrlPathSegment => @"Log";
		public IScreen HostScreen { get; }

		public LogViewModel(IScreen hostScreen)
		{
			HostScreen = hostScreen;
		}
	}
}
