using System.Text.Json.Serialization;

namespace TranslationsMigrator.Models
{
	public class JsonTranslationDto
	{
		[JsonPropertyName("German")]
		public string Origin { get; set; }
		
		public string Translation { get; set; }
	}
}