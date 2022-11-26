using slowpoke.core.Client;

namespace SlowPokeIMS.Integration.Tests.Core;


public interface ITestSlowPokeClient: ISlowPokeClient
{
    Task EnsureNoFilesOrFolders();
    Task CreateFolder(string folderPath, bool syncEnabled);
    Task CreateFile(string filePath, string contents, bool syncEnabled);
}