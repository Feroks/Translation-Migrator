using System.IO;

namespace TranslationMigrator.Infrastructure.Services
{
	public interface IFileService
	{
		FileStream Open(string path, FileMode mode, FileAccess access, FileShare share);
	}
}