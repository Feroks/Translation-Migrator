using MediatR;

namespace TranslationsMigrator.Commands
{
	public class CreateResourceFileRequest : IRequest
	{
		public CreateResourceFileRequest(Options options)
		{
			Options = options;
		}
		
		public Options Options { get; }
	}
}