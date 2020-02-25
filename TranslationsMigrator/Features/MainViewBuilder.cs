using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveUI;
using Terminal.Gui;

namespace TranslationsMigrator.Features
{
	public class MainViewBuilder
	{
		private readonly MainViewModel _mainViewModel;
		private readonly Window _window;
		private TextField? _destinationTextField;
		private TextField? _originTextField;
		private Button? _runButton;
		private TextField? _sourceTextField;

		public MainViewBuilder(MainViewModel mainViewModel)
		{
			_mainViewModel = mainViewModel;

			_window = new Window("Translation Migrator")
			{
				Width = Dim.Fill(),
				Height = Dim.Fill()
			};
		}

		/// <summary>
		/// Add UI components to <see cref="_window" />
		/// </summary>
		public MainViewBuilder SetupControls()
		{
			var sourceLabel = new Label("Source Translation JSON File:")
			{
				X = 0,
				Y = 1
			};
			_sourceTextField = new TextField(_mainViewModel.SourceFilePath)
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(sourceLabel) + 1
			};

			var originLabel = new Label("Origin Resource File:")
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(_sourceTextField) + 2
			};
			_originTextField = new TextField(_mainViewModel.OriginFilePath)
			{
				X = Pos.Left(originLabel),
				Y = Pos.Top(originLabel) + 1
			};

			var destinationLabel = new Label("Destination Resource File:")
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(_originTextField) + 4
			};
			_destinationTextField = new TextField(_mainViewModel.DestinationFilePath)
			{
				X = Pos.Left(destinationLabel),
				Y = Pos.Top(destinationLabel) + 1
			};

			_runButton = new Button("Run")
			{
				X = Pos.Center(),
				Y = Pos.AnchorEnd(2)
			};

			_window.Add(
				sourceLabel,
				_sourceTextField,
				originLabel,
				_originTextField,
				destinationLabel,
				_destinationTextField,
				_runButton);

			return this;
		}

		/// <summary>
		/// Bind values to ViewModel
		/// <remarks>Real binding are not available, therefore event subscriptions are used instead</remarks>
		/// </summary>
		public MainViewBuilder SetupBindings()
		{
			CreateTextFieldObservable(_sourceTextField!)
				.Subscribe(x => _mainViewModel.SourceFilePath = x);

			CreateTextFieldObservable(_originTextField!)
				.Subscribe(x => _mainViewModel.OriginFilePath = x);

			CreateTextFieldObservable(_destinationTextField!)
				.Subscribe(x => _mainViewModel.DestinationFilePath = x);

			_mainViewModel
				.ShowSuccessMessage
				.RegisterHandler(HandleShowSuccessMessage);

			_mainViewModel
				.ShowErrorMessage
				.RegisterHandler(HandleShowErrorMessage);

			_runButton!.Clicked = () => Observable
				.Return(Unit.Default)
				.InvokeCommand(_mainViewModel.Migrate);

			return this;
		}

		/// <summary>
		/// Create Main Window
		/// </summary>
		public View Build()
		{
			return _window;
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