namespace TranslationMigrator.Core
{
	public readonly struct ResourceValueDto
	{
		public ResourceValueDto(string key, string value)
		{
			Key = key;
			Value = value;
		}

		public string Key { get; }

		public string Value { get; }
	}
}