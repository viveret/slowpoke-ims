using slowpoke.core.Client;
using slowpoke.core.Models.Configuration;

namespace SlowPokeIMS.Integration.Tests.Core;


public class HttpTestSlowPokeClient : HttpSlowPokeClient, ITestSlowPokeClient
{
    protected HttpTestSlowPokeClient(Uri uri, Config config, HttpClient? client = null) : base(uri, config, client)
    {
    }

    public static async Task<ITestSlowPokeClient> CreateTestClient(Uri url, Config config, HttpClient? httpClient = null, CancellationToken cancellationToken = default)
    {
        var client = new HttpTestSlowPokeClient(url, config, httpClient);
        await client.Connect(cancellationToken);
        return client;
    } 

    public Task CreateFolder(string folderPath, bool syncEnabled)
    {
        return Query<bool>($"test/create-folder/{Uri.EscapeDataString(folderPath)}/{syncEnabled}", null, str => Task.FromResult(bool.Parse(str)), CancellationToken.None);
    }

    public Task CreateFile(string filePath, string content, bool syncEnabled)
    {
        return Query<bool>($"test/create-file/{Uri.EscapeDataString(filePath)}/{syncEnabled}", null, str => Task.FromResult(bool.Parse(str)), CancellationToken.None);
    }

    public Task EnsureNoFilesOrFolders()
    {
        return Query<bool>($"test/EnsureNoFilesOrFolders", null, str => Task.FromResult(bool.TryParse(str, out var b) ? b : throw new Exception(str)), CancellationToken.None);
    }
}