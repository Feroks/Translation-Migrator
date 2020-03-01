using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TranslationMigrator.Core;

namespace TranslationMigrator.Infrastructure.Services
{
	public interface IResourceService
	{
		ValueTask<IEnumerable<ResourceValueDto>> ReadAsync(string path, CancellationToken cancellationToken);

		ValueTask WriteAsync(string path, IEnumerable<ResourceValueDto> values, CancellationToken cancellationToken);
	}
}