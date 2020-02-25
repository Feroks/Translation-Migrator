using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terminal.Gui;
using TranslationsMigrator.Extensions;
using TranslationsMigrator.Views;

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
				var mediator = serviceProvider.GetRequiredService<IMediator>();
				var settings = serviceProvider.GetRequiredService<ISettings>();
				
				Application.Init();
				new ViewSetup(logger, mediator, settings).ComposeUi(Application.Top);
				Application.Run();
				
				logger.LogInformation("Application has exited");
			}
			catch (Exception e)
			{
				logger.LogCritical(e, "Application has crashed");
				throw;
			}
		}

		private static ServiceProvider BuildServiceProvider() =>
			new ServiceCollection()
				.AddServices()
				.AddCustomLogging()
				.AddCustomSettings()
				.AddMediatR(typeof(Program).Assembly)
				.BuildServiceProvider();
	}
}