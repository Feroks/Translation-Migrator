using System;
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
		private readonly ISettings _settings;

		public MainViewModel(ILogger<MainViewModel> logger, IMediator mediator, ISettings settings)
		{
			_mediator = mediator;
			_settings = settings;

			ShowSuccessMessage = new Interaction<RUnit, RUnit>();
			ShowErrorMessage = new Interaction<Exception, RUnit>();
			Migrate = ReactiveCommand.CreateFromTask(MigrateAsync);

			SourceFilePath = _settings.Source ?? string.Empty;
			OriginFilePath = _settings.Origin ?? string.Empty;
			DestinationFilePath = _settings.Destination ?? string.Empty;

			// Settings is a json File. Concurrent write will cause issues, therefore lock it via sync object
			var sync = new object();

			this.WhenAnyValue(x => x.SourceFilePath)
				.ObserveOn(RxApp.TaskpoolScheduler)
				.Synchronize(sync)
				.Subscribe(x => _settings.Source = x);

			this.WhenAnyValue(x => x.OriginFilePath)
				.ObserveOn(RxApp.TaskpoolScheduler)
				.Synchronize(sync)
				.Subscribe(x => _settings.Origin = x);

			this.WhenAnyValue(x => x.DestinationFilePath)
				.ObserveOn(RxApp.TaskpoolScheduler)
				.Synchronize(sync)
				.Subscribe(x => _settings.Destination = x);

			// Handle success
			Migrate
				.Do(_ => logger.LogInformation("Finished migrating Resource file"))
				.ObserveOn(RxApp.MainThreadScheduler)
				.SelectMany(x => ShowSuccessMessage.Handle(RUnit.Default))
				.Subscribe();

			// Handle error
			Migrate
				.ThrownExceptions
				.Do(e => logger.LogError(e, "Failed to migrate Resource file"))
				.ObserveOn(RxApp.MainThreadScheduler)
				.SelectMany(x => ShowErrorMessage.Handle(x))
				.Subscribe();
		}

		[Reactive]
		public string SourceFilePath { get; set; }

		[Reactive]
		public string OriginFilePath { get; set; }

		[Reactive]
		public string DestinationFilePath { get; set; }

		public ReactiveCommand<RUnit, RUnit> Migrate { get; }

		public Interaction<RUnit, RUnit> ShowSuccessMessage { get; }

		public Interaction<Exception, RUnit> ShowErrorMessage { get; }

		private async Task<RUnit> MigrateAsync(CancellationToken cancellationToken)
		{
			var request = new CreateResourceFileRequest(
				_settings.Source ?? string.Empty,
				_settings.Destination ?? string.Empty,
				_settings.Origin ?? string.Empty);

			await _mediator
				.Send(request, cancellationToken)
				.ConfigureAwait(false);

			return RUnit.Default;
		}
	}
}