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
		private static async Task Main(string[] args)
		{
			await using var serviceProvider = BuildServiceProvider();
			var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
		

			try
			{
				var mediator = serviceProvider.GetRequiredService<IMediator>();
				var settings = serviceProvider.GetRequiredService<ISettings>();
				
				Application.Init();
				var top = Application.Top;

				new ViewSetup(mediator, settings)
					.ComposeUi(top);
				
				Application.Run();
				
				// await Parser
				// 	.Default
				// 	.ParseArguments<Options>(args)
				// 	.MapResult(
				// 		options => mediator.Send(new CreateResourceFileRequest(options)),
				// 		_ => Task.CompletedTask);

				logger.LogInformation("Finished");
			}
			catch (Exception e)
			{
				logger.LogCritical(e, "Error has happened");
			}
			finally
			{
				logger.LogInformation("Press any key to exit...");
				Console.ReadKey();
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