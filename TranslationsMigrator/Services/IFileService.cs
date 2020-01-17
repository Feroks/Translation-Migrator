using System.IO;

namespace TranslationsMigrator.Services
{
	public interface IFileService
	{
		FileStream Open(string path, FileMode mode, FileAccess access, FileShare share);
	}
}