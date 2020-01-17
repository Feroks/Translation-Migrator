using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MediatR;
using Terminal.Gui;
using TranslationsMigrator.Commands;

namespace TranslationsMigrator.Views
{
	public class ViewSetup
	{
		private readonly IMediator _mediator;
		private readonly ISettings _settings;

		public ViewSetup(IMediator mediator, ISettings settings)
		{
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

			Observable
				.FromEventPattern<EventHandler, EventArgs>(
					h => sourceTextField.Changed += h,
					h => sourceTextField.Changed -= h)
				.ObserveOn(TaskPoolScheduler.Default)
				.Select(x => sourceTextField.Text.ToString())
				.Distinct()
				.Synchronize(sync)
				.Subscribe(x => _settings.Source = x);
			
			Observable
				.FromEventPattern<EventHandler, EventArgs>(
					h => destinationTextField.Changed += h,
					h => destinationTextField.Changed -= h)
				.ObserveOn(TaskPoolScheduler.Default)
				.Select(x => destinationTextField.Text.ToString())
				.Distinct()
				.Synchronize(sync)
				.Subscribe(x => _settings.Destination = x);
			
			Observable
				.FromEventPattern<EventHandler, EventArgs>(
					h => originTextField.Changed += h,
					h => originTextField.Changed -= h)
				.ObserveOn(TaskPoolScheduler.Default)
				.Select(x => originTextField.Text.ToString())
				.Distinct()
				.Synchronize(sync)
				.Subscribe(x => _settings.Origin = x);

			runButton.Clicked = RunClicked;
		}

		private void RunClicked()
		{
			Observable
				.FromAsync(() => _mediator
					.Send(new CreateResourceFileRequest(
						_settings.Source,
						_settings.Destination,
						_settings.Origin)))
				// Wait to prevent this method exiting before Async method finishes
				.Wait();

			// Show message about result
			MessageBox.Query(50, 7, "Success", "File was created!", "Ok");
		}
	}
}