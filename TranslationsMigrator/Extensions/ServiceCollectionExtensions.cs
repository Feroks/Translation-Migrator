using Config.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TranslationsMigrator.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddServices(this IServiceCollection services)
		{
			services.Scan(scan => scan
				.FromAssemblyOf<Program>()
				.AddClasses(classes => classes
					.Where(x => x.Name.EndsWith("Service")))
				.AsImplementedInterfaces()
				.WithSingletonLifetime());

			return services;
		}

		public static IServiceCollection AddCustomLogging(this IServiceCollection services)
		{
			services
				.AddLogging(configure => configure
					.ClearProviders()
					.AddConsole());

			return services;
		}

		public static IServiceCollection AddCustomSettings(this IServiceCollection services)
		{
			services.AddSingleton(_ => new ConfigurationBuilder<ISettings>()
				.UseJsonFile("settings.json")
				.Build());

			return services;
		}
	}
}