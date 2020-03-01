using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terminal.Gui;
using TranslationMigrator.App.Extensions;
using TranslationMigrator.App.Features.Main;

namespace TranslationMigrator.App
{
	[UsedImplicitly]
	internal class Program
	{
		private static async Task Main()
		{
			Application.Init();

			await using var serviceProvider = BuildServiceProvider();
			var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

			try
			{
				var view = serviceProvider
					.GetRequiredService<MainViewBuilder>()
					.SetupControls()
					.SetupBindings()
					.Build();

				Application.Top.Add(view);
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