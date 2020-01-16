using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TranslationsMigrator.Models;

namespace TranslationsMigrator.Services
{
	public class JsonTranslationService : IJsonTranslationService
	{
		public async ValueTask<IEnumerable<JsonTranslationDto>> ReadAsync(string path,
			CancellationToken cancellationToken = default)
		{
			await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			return await JsonSerializer
				.DeserializeAsync<IEnumerable<JsonTranslationDto>>(stream, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
	}
}