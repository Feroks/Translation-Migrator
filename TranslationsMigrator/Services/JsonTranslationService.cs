using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TranslationsMigrator.Models;

namespace TranslationsMigrator.Services
{
	[UsedImplicitly]
	public class JsonTranslationService : IJsonTranslationService
	{
		private readonly IFileService _fileService;

		public JsonTranslationService(IFileService fileService)
		{
			_fileService = fileService;
		}

		public async ValueTask<IEnumerable<JsonTranslationDto>> ReadAsync(string path,
			CancellationToken cancellationToken = default)
		{
			await using var stream = _fileService.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			return await JsonSerializer
				.DeserializeAsync<IEnumerable<JsonTranslationDto>>(stream, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
	}
}