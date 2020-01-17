using System;
using System.Threading.Tasks;
using CommandLine;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TranslationsMigrator.Commands;
using TranslationsMigrator.Extensions;

namespace TranslationsMigrator
{
	[UsedImplicitly]
	internal class Program
	{
		private static async Task Main(string[] args)
		{
			await using var serviceProvider = BuildServiceProvider();
			var logger = serviceProvider.GetService<ILogger<Program>>();
			var mediator = serviceProvider.GetService<IMediator>();

			try
			{
				await Parser
					.Default
					.ParseArguments<Options>(args)
					.MapResult(
						options => mediator.Send(new CreateResourceFileRequest(options)),
						_ => Task.CompletedTask);

				logger.LogInformation("Finished. Press any key to exit...");
			}
			catch (Exception e)
			{
				logger.LogCritical(e, "Error has happened");
			}
			finally
			{
				Console.ReadKey();
			}
		}

		private static ServiceProvider BuildServiceProvider() =>
			new ServiceCollection()
				.AddServices()
				.AddCustomLogging()
				.AddMediatR(typeof(Program).Assembly)
				.BuildServiceProvider();
	}
}