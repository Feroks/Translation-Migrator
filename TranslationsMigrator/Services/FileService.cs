using System.IO;
using JetBrains.Annotations;

namespace TranslationsMigrator.Services
{
	[UsedImplicitly]
	public class FileService : IFileService
	{
		public FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return new FileStream(path, mode, access, share);
		}
	}
}