namespace TranslationsMigrator
{
	public interface ISettings
	{
		string? OriginFolderPath { get; set; }
		
		string? SourceFilePath { get; set; }
		
		string? DestinationFolderPath { get; set; }
	}
}