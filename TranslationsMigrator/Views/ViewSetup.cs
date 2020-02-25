using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Terminal.Gui;
using TranslationsMigrator.Commands;
using MUnit = MediatR.Unit;
using RUnit = System.Reactive.Unit;

namespace TranslationsMigrator.Views
{
	public class ViewSetup
	{
		private readonly ILogger _logger;
		private readonly IMediator _mediator;
		private readonly ISettings _settings;
		private readonly ReactiveCommand<RUnit, RUnit> _migrate;

		public ViewSetup(ILogger logger, IMediator mediator, ISettings settings)
		{
			_logger = logger;
			_mediator = mediator;
			_settings = settings;
			_migrate = ReactiveCommand.CreateFromTask(MigrateAsync);
		}

		public void ComposeUi(Toplevel top)
		{
			var window = new Window("Translation Migrator")
			{
				Width = Dim.Fill(),
				Height = Dim.Fill()
			};
			top.Add(window);

			var sourceLabel = new Label("Source Translation JSON File:")
			{
				X = 0,
				Y = 1
			};
			var sourceTextField = new TextField(_settings.Source ?? string.Empty)
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(sourceLabel) + 1
			};

			var originLabel = new Label("Origin Resource File:")
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(sourceTextField) + 2
			};
			var originTextField = new TextField(_settings.Origin ?? string.Empty)
			{
				X = Pos.Left(originLabel),
				Y = Pos.Top(originLabel) + 1
			};

			var destinationLabel = new Label("Destination Resource File:")
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(originTextField) + 4
			};
			var destinationTextField = new TextField(_settings.Destination ?? string.Empty)
			{
				X = Pos.Left(destinationLabel),
				Y = Pos.Top(destinationLabel) + 1
			};

			var runButton = new Button("Run")
			{
				X = Pos.Center(),
				Y = Pos.AnchorEnd(2)
			};

			window.Add(
				sourceLabel,
				sourceTextField,
				originLabel,
				originTextField,
				destinationLabel,
				destinationTextField,
				runButton);

			SetupEvents(sourceTextField, destinationTextField, originTextField, runButton);
		}

		private void SetupEvents(
			TextField sourceTextField,
			TextField destinationTextField,
			TextField originTextField,
			Button runButton)
		{
			// Settings is a json File. Concurrent write will cause issues, therefore lock it via sync object
			var sync = new object();

			IObservable<string> CreateObservableOnTextField(TextField textField)
			{
				return Observable
					.FromEventPattern<EventHandler, EventArgs>(
						h => textField.Changed += h,
						h => textField.Changed -= h)
					.ObserveOn(TaskPoolScheduler.Default)
					.Select(x => x.Sender)
					.Cast<TextField>()
					.Select(x => x.Text.ToString() ?? string.Empty)
					.DistinctUntilChanged()
					.Throttle(TimeSpan.FromMilliseconds(10))
					.Synchronize(sync);
			}

			// Setup persistence to config file
			CreateObservableOnTextField(sourceTextField)
				.Subscribe(x => _settings.Source = x);

			CreateObservableOnTextField(destinationTextField)
				.Subscribe(x => _settings.Destination = x);

			CreateObservableOnTextField(originTextField)
				.Subscribe(x => _settings.Origin = x);

			// Show Success message
			_migrate
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(_ => MessageBox.Query(50, 7, "Success", "File was created!", "Ok"));

			// Handle Error
			_migrate
				.ThrownExceptions
				.Do(e => _logger.LogError(e, "Failed to create resource file"))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(e => MessageBox.ErrorQuery(100, 10, "Error", e.Message, "Ok"));

			runButton.Clicked = () => Observable
				.Return(RUnit.Default)
				.InvokeCommand(_migrate);
		}

		private async Task<System.Reactive.Unit> MigrateAsync(CancellationToken cancellationToken)
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