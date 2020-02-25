using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using TranslationsMigrator.Models;
using TranslationsMigrator.Services;

namespace TranslationsMigrator.Commands
{
	[UsedImplicitly]
	public class CreateResourceFileRequestHandler : AsyncRequestHandler<CreateResourceFileRequest>
	{
		private readonly ILogger<CreateResourceFileRequestHandler> _logger;
		private readonly IJsonTranslationService _jsonTranslationService;
		private readonly IResourceService _resourceService;

		public CreateResourceFileRequestHandler(
			ILogger<CreateResourceFileRequestHandler> logger,
			IJsonTranslationService jsonTranslationService,
			IResourceService resourceService)
		{
			_logger = logger;
			_jsonTranslationService = jsonTranslationService;
			_resourceService = resourceService;
		}

		protected override async Task Handle(CreateResourceFileRequest request, CancellationToken cancellationToken)
		{
			var sourceFilePath = request.SourceFilePath;
			var originFilePath = request.OriginFilePath;
			var destinationFilePath = request.DestinationFilePath;

			_logger.LogInformation("Starting Migration");
			
			_logger.LogInformation("Reading Translation file at: {filePath}", sourceFilePath);
			var translations = await _jsonTranslationService
				.ReadAsync(sourceFilePath, cancellationToken)
				.ConfigureAwait(false);

			_logger.LogInformation("Reading Origin Resource file at: {filePath}", originFilePath);
			var resourceValues = await _resourceService
				.ReadAsync(originFilePath, cancellationToken)
				.ConfigureAwait(false);

			_logger.LogInformation("Writing Resource file at: {filePath}", destinationFilePath);
			var values = resourceValues
				.Select(x =>
				{
					var value = translations
						.SingleOrDefault(y => y.Origin == x.Value)
						?.Translation;

					return new ResourceValueDto(x.Key, value ?? string.Empty);
				});

			await _resourceService
				.WriteAsync(destinationFilePath, values, cancellationToken)
				.ConfigureAwait(false);
		}
	}
}