using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TranslationsMigrator.Extentions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddServices(this IServiceCollection services)
		{
			services.Scan(scan => scan
				.FromExecutingAssembly()
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
	}
}