using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TranslationMigrator.Core;

namespace TranslationMigrator.Infrastructure.Services
{
	public interface IJsonTranslationService
	{
		ValueTask<IEnumerable<JsonTranslationDto>>
			ReadAsync(string path, CancellationToken cancellationToken = default);
	}
}