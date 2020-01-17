using CommandLine;
using JetBrains.Annotations;

namespace TranslationsMigrator
{
	public class Settings : ISettings
	{
		[Option('o', "origin", Required = true, HelpText = "Path to German Resource File")]
		public string Origin { get; [UsedImplicitly] set; } = string.Empty;
		
		[Option('s', "source", Required = true, HelpText = "Source path for JSON File")]
		public string Source { get; [UsedImplicitly] set; } = string.Empty;

		[Option('d', "destination", Required = true, HelpText = "Destination path for Resource File")]
		public string Destination { get; [UsedImplicitly] set; } = string.Empty;
	}
}