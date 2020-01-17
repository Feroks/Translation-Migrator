using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using Terminal.Gui;
using TranslationsMigrator.Commands;

namespace TranslationsMigrator.Views
{
	public class ViewSetup
	{
		private readonly ILogger _logger;
		private readonly IMediator _mediator;
		private readonly ISettings _settings;

		public ViewSetup(ILogger logger, IMediator mediator, ISettings settings)
		{
			_logger = logger;
			_mediator = mediator;
			_settings = settings;
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
			// Sync is used to prevent concurrent write to Settings
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
					.Select(x => x.Text.ToString())
					.DistinctUntilChanged()
					.Throttle(TimeSpan.FromMilliseconds(10))
					.Synchronize(sync);
			}

			CreateObservableOnTextField(sourceTextField)
				.Subscribe(x => _settings.Source = x);
			
			CreateObservableOnTextField(destinationTextField)
				.Subscribe(x => _settings.Destination = x);

			CreateObservableOnTextField(originTextField)
				.Subscribe(x => _settings.Origin = x);

			runButton.Clicked = RunClicked;
		}

		private void RunClicked()
		{
			try
			{
				Observable
					.FromAsync(() => _mediator
						.Send(new CreateResourceFileRequest(
							_settings.Source ?? string.Empty,
							_settings.Destination ?? string.Empty,
							_settings.Origin ?? string.Empty)))
					// Wait to prevent this method exiting before Async method finishes
					.Wait();
				
				MessageBox.Query(50, 7, "Success", "File was created!", "Ok");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to create resource file");
				MessageBox.ErrorQuery(100, 10, "Error", e.Message, "Ok");
			}
		}
	}
}