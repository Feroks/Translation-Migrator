using MediatR;

namespace TranslationsMigrator.Commands
{
	public class CreateResourceFileRequest : IRequest
	{
		public CreateResourceFileRequest(Settings settings)
		{
			Settings = settings;
		}
		
		public Settings Settings { get; }
	}
}