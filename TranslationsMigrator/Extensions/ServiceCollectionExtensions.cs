using Config.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TranslationsMigrator.Features;

namespace TranslationsMigrator.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddServices(this IServiceCollection services)
		{
			return services.Scan(scan => scan
				.FromAssemblyOf<Program>()
				.AddClasses(classes => classes
					.Where(x => x.Name.EndsWith("Service")))
				.AsImplementedInterfaces()
				.WithSingletonLifetime());
		}

		public static IServiceCollection AddCustomLogging(this IServiceCollection services)
		{
			return services
				.AddLogging(configure => configure
					.ClearProviders()
					.AddConsole());
		}

		public static IServiceCollection AddCustomSettings(this IServiceCollection services)
		{
			return services.AddSingleton(_ => new ConfigurationBuilder<ISettings>()
				.UseJsonFile("settings.json")
				.Build());
		}

		public static IServiceCollection AddViews(this IServiceCollection services)
		{
			return services.AddTransient<MainViewBuilder, MainViewBuilder>();
		}

		public static IServiceCollection AddViewModels(this IServiceCollection services)
		{
			return services.AddTransient<MainViewModel, MainViewModel>();
		}
	}
}