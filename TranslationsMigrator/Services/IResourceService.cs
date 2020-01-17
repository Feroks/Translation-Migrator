using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TranslationsMigrator.Models;

namespace TranslationsMigrator.Services
{
	public interface IResourceService
	{
		ValueTask<IEnumerable<ResourceValueDto>> ReadAsync(string path, CancellationToken cancellationToken);
		
		ValueTask WriteAsync(string path, IEnumerable<ResourceValueDto> values, CancellationToken cancellationToken);
	}
}