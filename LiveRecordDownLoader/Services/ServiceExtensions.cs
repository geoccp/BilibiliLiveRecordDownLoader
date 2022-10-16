using Api.Clients;
using Api.Model.LiveRecordList;
using LiveRecordDownLoader.FFmpeg;
using LiveRecordDownLoader.FlvProcessor.Clients;
using LiveRecordDownLoader.FlvProcessor.Interfaces;
using LiveRecordDownLoader.Http.Clients;
using LiveRecordDownLoader.Interfaces;
using LiveRecordDownLoader.Models;
using LiveRecordDownLoader.Models.TaskViewModels;
using LiveRecordDownLoader.Shared.Utils;
using LiveRecordDownLoader.ViewModels;
using LiveRecordDownLoader.Views;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Punchclock;
using ReactiveUI;
using RunAtStartup;

namespace LiveRecordDownLoader.Services
{
	public static class ServiceExtensions
	{
		public static IServiceCollection AddViewModels(this IServiceCollection services)
		{
			services.TryAddSingleton<MainWindowViewModel>();
			services.TryAddSingleton<LiveRecordListViewModel>();
			services.TryAddSingleton<TaskListViewModel>();
			services.TryAddSingleton<LogViewModel>();
			services.TryAddSingleton<SettingViewModel>();
			services.TryAddSingleton<StreamRecordViewModel>();
			services.TryAddSingleton<UserSettingsViewModel>();
			services.TryAddSingleton<FFmpegCommandViewModel>();

			services.TryAddSingleton<IScreen>(provider => provider.GetRequiredService<MainWindowViewModel>());

			return services;
		}

		public static IServiceCollection AddViews(this IServiceCollection services)
		{
			services.TryAddSingleton<MainWindow>();
			services.TryAddTransient<IViewFor<LiveRecordListViewModel>, LiveRecordListView>();
			services.TryAddTransient<IViewFor<TaskListViewModel>, TaskListView>();
			services.TryAddTransient<IViewFor<LogViewModel>, LogView>();
			services.TryAddTransient<IViewFor<SettingViewModel>, SettingView>();
			services.TryAddTransient<IViewFor<StreamRecordViewModel>, StreamRecordView>();
			services.TryAddTransient<IViewFor<UserSettingsViewModel>, UserSettingsView>();
			services.TryAddTransient<IViewFor<FFmpegCommandViewModel>, FFmpegCommandView>();

			return services;
		}

		public static IServiceCollection AddDanmuClients(this IServiceCollection services)
		{
			services.TryAddTransient<TcpDanmuClient>();
			services.TryAddTransient<WsDanmuClient>();
			services.TryAddTransient<WssDanmuClient>();

			return services;
		}

		public static IServiceCollection AddConfig(this IServiceCollection services)
		{
			services.TryAddSingleton<IConfigService, ConfigService>();
			services.TryAddSingleton<Config>();

			return services;
		}

		public static IServiceCollection AddDynamicData(this IServiceCollection services)
		{
			services.TryAddSingleton<SourceList<LiveRecordList>>();
			services.TryAddSingleton<SourceList<RoomStatus>>();
			services.TryAddSingleton<SourceList<TaskViewModel>>();

			return services;
		}

		public static IServiceCollection AddFlvProcessor(this IServiceCollection services)
		{
			services.TryAddTransient<IFlvMerger, FlvMerger>();
			services.TryAddTransient<IFlvExtractor, FlvExtractor>();
			services.TryAddTransient<FFmpegCommand>();

			return services;
		}

		public static IServiceCollection AddStartupService(this IServiceCollection services)
		{
			services.TryAddSingleton(new StartupService(nameof(LiveRecordDownLoader)));

			return services;
		}

		public static IServiceCollection AddGlobalTaskQueue(this IServiceCollection services)
		{
			services.TryAddSingleton(new OperationQueue(int.MaxValue));

			return services;
		}

		public static IServiceCollection AddBilibiliApiClient(this IServiceCollection services)
		{
			services.TryAddSingleton(provider =>
			{
				var config = provider.GetRequiredService<Config>();
				var client = HttpClientUtils.BuildClientForBilibili(config.UserAgent, config.Cookie, config.HttpHandler);
				return new BilibiliApiClient(client);
			});

			return services;
		}

		public static IServiceCollection AddHttpDownloader(this IServiceCollection services)
		{
			services.TryAddTransient(provider =>
			{
				var config = provider.GetRequiredService<Config>();
				var client = HttpClientUtils.BuildClientForBilibili(config.UserAgent, config.Cookie, config.HttpHandler);
				return new HttpDownloader(client);
			});

			services.TryAddTransient(provider =>
			{
				var logger = provider.GetRequiredService<ILogger<MultiThreadedDownloader>>();
				var config = provider.GetRequiredService<Config>();
				var client = HttpClientUtils.BuildClientForMultiThreadedDownloader(config.Cookie, config.UserAgent, config.HttpHandler);
				return new MultiThreadedDownloader(logger, client);
			});

			return services;
		}
	}
}
