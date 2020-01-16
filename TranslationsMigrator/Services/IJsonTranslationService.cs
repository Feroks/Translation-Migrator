using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TranslationsMigrator.Models;

namespace TranslationsMigrator.Services
{
	public interface IJsonTranslationService
	{
		ValueTask<IEnumerable<JsonTranslationDto>> ReadAsync(string path, CancellationToken cancellationToken = default);
	}
}