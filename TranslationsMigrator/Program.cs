using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terminal.Gui;
using TranslationsMigrator.Extensions;
using TranslationsMigrator.Features;

namespace TranslationsMigrator
{
	[UsedImplicitly]
	internal class Program
	{
		private static async Task Main()
		{
			await using var serviceProvider = BuildServiceProvider();
			var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

			try
			{
				Application.Init();

				serviceProvider
					.GetRequiredService<MainView>()
					.ComposeUi(Application.Top);

				Application.Run();

				logger.LogInformation("Application has exited");
			}
			catch (Exception e)
			{
				logger.LogCritical(e, "Application has crashed");
				throw;
			}
		}

		private static ServiceProvider BuildServiceProvider()
		{
			return new ServiceCollection()
				.AddViews()
				.AddViewModels()
				.AddServices()
				.AddCustomLogging()
				.AddCustomSettings()
				.AddMediatR(typeof(Program).Assembly)
				.BuildServiceProvider();
		}
	}
}