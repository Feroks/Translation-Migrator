using System;

namespace TranslationsMigrator
{
	public interface ISettings
	{
		string? Origin { get; set; }
		
		string? Source { get; set; }
		
		string? Destination { get; set; }
	}
}