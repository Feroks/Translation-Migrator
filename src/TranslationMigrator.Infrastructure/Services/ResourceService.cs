using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using JetBrains.Annotations;
using TranslationMigrator.Core;

namespace TranslationMigrator.Infrastructure.Services
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

		public async ValueTask<IEnumerable<ResourceValueDto>> ReadAsync(
			string path,
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
						.Attributes(NameAttributeName)
						.Single()
						.Value;

					var value = x
						.Descendants(ValueTagName)
						.Single()
						.Value;

					return new ResourceValueDto(key, value);
				});
		}

		public async ValueTask WriteAsync(
			string path,
			IEnumerable<ResourceValueDto> values,
			CancellationToken cancellationToken)
		{
			// If Header Elements are not present, MSBUILD throws error
			var headerElements = await GetHeaderElementsAsync(cancellationToken)
				.ConfigureAwait(false);

			var document = new XDocument(
				new XElement(
					"root",
					headerElements,
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

		[ItemNotNull]
		private async ValueTask<IEnumerable<XElement>> GetHeaderElementsAsync(CancellationToken cancellationToken)
		{
			await using var stream = Assembly
				.GetExecutingAssembly()
				.GetManifestResourceStream("TranslationMigrator.Infrastructure.Resources.Header.xml");

			if (stream == null)
				throw new ArgumentNullException(nameof(stream), "Could not find Header resource");

			var document = await XDocument
				.LoadAsync(stream, LoadOptions.None, cancellationToken)
				.ConfigureAwait(false);

			var elements = document
				.Root
				?.Elements();

			return elements ?? Enumerable.Empty<XElement>();
		}
	}
}