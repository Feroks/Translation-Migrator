using MediatR;
using Terminal.Gui;

namespace TranslationsMigrator.Views
{
	public class ViewSetup
	{
		private readonly IMediator _mediator;

		public ViewSetup(IMediator mediator, ISettings settings)
		{
			_mediator = mediator;
		}

		public ViewSetup ComposeUi(Toplevel top)
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
			var sourceTextField = new TextField(string.Empty)
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(sourceLabel) + 1
			};

			var originLabel = new Label("Origin Resource File:")
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(sourceTextField) + 2
			};
			var originTextField = new TextField(string.Empty)
			{
				X = Pos.Left(originLabel),
				Y = Pos.Top(originLabel) + 1
			};

			var destinationLabel = new Label("Destination Resource File:")
			{
				X = Pos.Left(sourceLabel),
				Y = Pos.Top(originTextField) + 4
			};
			var destinationTextField = new TextField(string.Empty)
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

			return this;
		}
	}
}