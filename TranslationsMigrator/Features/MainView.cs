using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveUI;
using Terminal.Gui;

namespace TranslationsMigrator.Views
{
	public class MainView
	{
		private readonly MainViewModel _mainViewModel;

		public MainView(MainViewModel mainViewModel)
		{
			_mainViewModel = mainViewModel;
		}

		/// <summary>
		/// Add UI components to <paramref name="top" />
		/// </summary>
		/// <param name="top"></param>
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
			var sourceTextField = new TextField(_mainViewModel.SourceFilePath)
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(sourceLabel) + 1
			};

			var originLabel = new Label("Origin Resource File:")
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(sourceTextField) + 2
			};
			var originTextField = new TextField(_mainViewModel.OriginFilePath)
			{
				X = Pos.Left(originLabel),
				Y = Pos.Top(originLabel) + 1
			};

			var destinationLabel = new Label("Destination Resource File:")
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(originTextField) + 4
			};
			var destinationTextField = new TextField(_mainViewModel.DestinationFilePath)
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

			SetupBindings(sourceTextField, originTextField, destinationTextField, runButton);
		}

		/// <summary>
		/// Bind values to ViewModel
		/// <remarks>Real binding is not available, therefore event subscriptions are used</remarks>
		/// </summary>
		private void SetupBindings(TextField sourceTextField,
			TextField originTextField,
			TextField destinationTextField,
			Button runButton)
		{
			CreateTextFieldObservable(sourceTextField)
				.Subscribe(x => _mainViewModel.SourceFilePath = x);

			CreateTextFieldObservable(originTextField)
				.Subscribe(x => _mainViewModel.OriginFilePath = x);

			CreateTextFieldObservable(destinationTextField)
				.Subscribe(x => _mainViewModel.DestinationFilePath = x);

			_mainViewModel
				.ShowSuccessMessage
				.RegisterHandler(HandleShowSuccessMessage);

			_mainViewModel
				.ShowErrorMessage
				.RegisterHandler(HandleShowErrorMessage);

			runButton.Clicked = () => Observable
				.Return(Unit.Default)
				.InvokeCommand(_mainViewModel.Migrate);
		}

		private static void HandleShowSuccessMessage(InteractionContext<Unit, Unit> interaction)
		{
			MessageBox.Query(50, 7, "Success", "File was created!", "Ok");
			interaction.SetOutput(Unit.Default);
		}

		private static void HandleShowErrorMessage(InteractionContext<Exception, Unit> interaction)
		{
			MessageBox.ErrorQuery(100, 10, "Error", interaction.Input.Message, "Ok");
			interaction.SetOutput(Unit.Default);
		}

		private static IObservable<string> CreateTextFieldObservable(TextField textField)
		{
			return Observable
				.FromEventPattern<EventHandler, EventArgs>(
					h => textField.Changed += h,
					h => textField.Changed -= h)
				.ObserveOn(TaskPoolScheduler.Default)
				.Select(x => x.Sender)
				.Cast<TextField>()
				.Select(x => x.Text.ToString() ?? string.Empty)
				.DistinctUntilChanged();
		}
	}
}