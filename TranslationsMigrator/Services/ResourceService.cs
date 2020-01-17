using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using JetBrains.Annotations;
using TranslationsMigrator.Models;

namespace TranslationsMigrator.Services
{
	[UsedImplicitly]
	public class ResourceService : IResourceService
	{
		private const string DataTagName = "data";
		private const string ValueTagName = "value";
		private const string NameAttributeName = "name";
		private readonly IFileService _fileService;

		public ResourceService(IFileService fileService)
		{
			_fileService = fileService;
		}

		public async ValueTask<IEnumerable<ResourceValueDto>> ReadAsync(string path,
			CancellationToken cancellationToken)
		{
			await using var steam = _fileService.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			var document = await XDocument
				.LoadAsync(steam, LoadOptions.None, cancellationToken)
				.ConfigureAwait(false);

			return document
				.Descendants(DataTagName)
				.Select(x =>
				{
					var key = x
						.Attributes()
						.Single(y => y.Name == NameAttributeName)
						.Value;

					var value = x
						.Descendants(ValueTagName)
						.Single()
						.Value;

					return new ResourceValueDto(key, value);
				});
		}

		public async ValueTask WriteAsync(string path, IEnumerable<ResourceValueDto> values,
			CancellationToken cancellationToken)
		{
			var document = new XDocument(
				new XElement(
					"root",
					values
						.Select(x =>
							new XElement(
								DataTagName,
								new XAttribute(
									NameAttributeName,
									x.Key),
								new XElement(
									ValueTagName,
									x.Value)
							)
						)
				)
			);

			await using var stream = _fileService.Open(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

			await document
				.SaveAsync(stream, SaveOptions.None, cancellationToken)
				.ConfigureAwait(false);
		}
	}
}