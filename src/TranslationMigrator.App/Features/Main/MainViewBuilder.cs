using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using NStack;
using ReactiveUI;
using Terminal.Gui;

namespace TranslationMigrator.App.Features.Main
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
        ///     Add UI components to <see cref="_window" />
        /// </summary>
        public MainViewBuilder SetupControls()
        {
            var sourceLabel = new Label("Source Translation JSON Folder:")
            {
                X = 0,
                Y = 1
            };
            _sourceTextField = new TextField(_mainViewModel.SourceFolderPath)
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

            var destinationLabel = new Label("Destination Resource Folder:")
            {
                X = Pos.Left(sourceLabel),
                Y = Pos.Top(_originTextField) + 4
            };
            _destinationTextField = new TextField(_mainViewModel.DestinationFolderPath)
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
        ///     Bind values to ViewModel
        ///     <remarks>Real binding are not available, therefore event subscriptions are used instead</remarks>
        /// </summary>
        public MainViewBuilder SetupBindings()
        {
            CreateTextFieldObservable(_sourceTextField!)
                .Subscribe(x => _mainViewModel.SourceFolderPath = x);

            CreateTextFieldObservable(_originTextField!)
                .Subscribe(x => _mainViewModel.OriginFilePath = x);

            CreateTextFieldObservable(_destinationTextField!)
                .Subscribe(x => _mainViewModel.DestinationFolderPath = x);

            _mainViewModel
                .ShowSuccessMessage
                .RegisterHandler(HandleShowSuccessMessage);

            _mainViewModel
                .ShowErrorMessage
                .RegisterHandler(HandleShowErrorMessage);

            _runButton!.Clicked = () => Observable
                .Return(Unit.Default)
                .InvokeCommand(_mainViewModel.CreateResourceFiles);

            return this;
        }

        /// <summary>
        ///     Create Main Window
        /// </summary>
        public View Build()
        {
            return _window;
        }

        private static void HandleShowSuccessMessage(InteractionContext<IReadOnlyCollection<string>, Unit> interaction)
        {
            var fileNames = interaction.Input;

            // We do not want list of filenames to overlap whole message box
            // Therefore use count. 5 is additional number of lines (for title, button and margins)
            var height = fileNames.Count + 5;

            // Same approach for Width
            var width = fileNames
                // 10 adds additional spaces to the left and to the right
                .Select(x => x.Length + 10)
                // This ensures Min Width, and avoids exception if collection is empty
                .Append(50)
                .Max();

            var message = string.Join(Environment.NewLine, fileNames);

            MessageBox.Query(width, height, "Following files were created:", message, "Ok");
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
                .FromEventPattern<ustring>(
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