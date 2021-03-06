﻿using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace TranslationMigrator.Core
{
	public class JsonTranslationDto
	{
		[JsonPropertyName("German")]
		public string Origin { get; [UsedImplicitly] set; } = string.Empty;

		public string Translation { get; [UsedImplicitly] set; } = string.Empty;
	}
}