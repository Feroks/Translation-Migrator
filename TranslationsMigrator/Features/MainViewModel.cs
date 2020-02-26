using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TranslationsMigrator.Commands;
using RUnit = System.Reactive.Unit;

namespace TranslationsMigrator.Features
{
	public class MainViewModel : ReactiveObject
	{
		private readonly IMediator _mediator;

		public MainViewModel(ILogger<MainViewModel> logger, IMediator mediator, ISettings settings)
		{
			_mediator = mediator;
			
			SourceFolderPath = settings.SourceFilePath ?? string.Empty;
			OriginFilePath = settings.OriginFolderPath ?? string.Empty;
			DestinationFolderPath = settings.DestinationFolderPath ?? string.Empty;

			ShowSuccessMessage = new Interaction<IReadOnlyCollection<string>, RUnit>();
			ShowErrorMessage = new Interaction<Exception, RUnit>();
			CreateResourceFiles = ReactiveCommand.CreateFromTask(CreateResourceFilesAsync);

			// Settings is a json File. Concurrent write will cause issues, therefore lock it via sync object
			var sync = new object();

			this.WhenAnyValue(x => x.SourceFolderPath)
				.ObserveOn(RxApp.TaskpoolScheduler)
				.Synchronize(sync)
				.Subscribe(x => settings.SourceFilePath = x);

			this.WhenAnyValue(x => x.OriginFilePath)
				.ObserveOn(RxApp.TaskpoolScheduler)
				.Synchronize(sync)
				.Subscribe(x => settings.OriginFolderPath = x);

			this.WhenAnyValue(x => x.DestinationFolderPath)
				.ObserveOn(RxApp.TaskpoolScheduler)
				.Synchronize(sync)
				.Subscribe(x => settings.DestinationFolderPath = x);

			// Handle success
			CreateResourceFiles
				.Do(_ => logger.LogInformation("Finished migrating Resource file"))
				.ObserveOn(RxApp.MainThreadScheduler)
				.SelectMany(ShowSuccessMessage.Handle)
				.Subscribe();

			// Handle error
			CreateResourceFiles
				.ThrownExceptions
				.Do(e => logger.LogError(e, "Failed to migrate Resource file"))
				.ObserveOn(RxApp.MainThreadScheduler)
				.SelectMany(ShowErrorMessage.Handle)
				.Subscribe();
		}

		[Reactive]
		public string SourceFolderPath { get; set; }

		[Reactive]
		public string OriginFilePath { get; set; }

		[Reactive]
		public string DestinationFolderPath { get; set; }

		public ReactiveCommand<RUnit, IReadOnlyCollection<string>> CreateResourceFiles { get; }

		public Interaction<IReadOnlyCollection<string>, RUnit> ShowSuccessMessage { get; }

		public Interaction<Exception, RUnit> ShowErrorMessage { get; }

		/// <summary>
		/// Generate Resource file 
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>List of File Paths for created files</returns>
		private async Task<IReadOnlyCollection<string>> CreateResourceFilesAsync(CancellationToken cancellationToken)
		{
			var tasks = Directory
				.EnumerateFiles(SourceFolderPath, "*.json", SearchOption.TopDirectoryOnly)
				.Select(async filePath =>
				{
					var destinationFilePath = CreateDestinationFilePath(filePath);

					var request = new CreateResourceFileRequest(
						filePath,
						destinationFilePath,
						OriginFilePath);

					await _mediator
						.Send(request, cancellationToken)
						.ConfigureAwait(false);

					return Path.GetFileName(destinationFilePath);
				});

			var filePaths = await Task
				.WhenAll(tasks)
				.ConfigureAwait(false);

			return filePaths.Length != 0
				? filePaths
				: throw new ArgumentException("Source folder does not contain JSON files", nameof(SourceFolderPath));
		}

		/// <summary>
		/// Create full destination File Path
		/// <example>....\Translations.en.resx</example>
		/// </summary>
		/// <param name="filePath">Path to original JSON file</param>
		private string CreateDestinationFilePath(string filePath)
		{
			var languageCode = GetLanguageCodeFromFileName(filePath);
			return Path.Combine(DestinationFolderPath, $"Translations.{languageCode.ToLower()}.resx");
		}

		/// <summary>
		/// Get language code based on File Name
		/// <example>EN, DE, FR</example>
		/// </summary>
		/// <param name="filePath">File Path that contains language code</param>
		private static string GetLanguageCodeFromFileName(string filePath)
		{
			var fileName = Path.GetFileNameWithoutExtension(filePath);
			return fileName.Substring(fileName.Length - 2);
		}
	}
}