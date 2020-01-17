using MediatR;

namespace TranslationsMigrator.Commands
{
	public class CreateResourceFileRequest : IRequest
	{
		public CreateResourceFileRequest(string sourceFilePath, string destinationFilePath, string originFilePath)
		{
			SourceFilePath = sourceFilePath;
			DestinationFilePath = destinationFilePath;
			OriginFilePath = originFilePath;
		}
		
		public string SourceFilePath { get; }
		
		public string DestinationFilePath { get; }
		
		public string OriginFilePath { get; }
	}
}