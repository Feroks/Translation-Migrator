using CommandLine;

namespace TranslationsMigrator
{
	public class Options
	{
		[Option('o', "origin", Required = true, HelpText = "Path to German Resource File")]
		public string Origin { get; set; }
		
		[Option('s', "source", Required = true, HelpText = "Source path for JSON File")]
		public string Source { get; set; }

		[Option('d', "destination", Required = true, HelpText = "Destination path for Resource File")]
		public string Destination { get; set; }
	}
}