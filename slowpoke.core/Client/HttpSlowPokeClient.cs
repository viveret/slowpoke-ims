using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;
using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.SyncState;

namespace slowpoke.core.Client;


public class HttpSlowPokeClient : ISlowPokeClient
{
    public static X509Certificate2? SystemCertificate { get; set; }

    private static HttpMessageHandler CreateHandler()
    {
        return new HttpClientHandler {
            ServerCertificateCustomValidationCallback = 
            (request, serverCertificate, serverChain, sslPolicyErrors) => {
                return true;// serverCertificate.Thumbprint == SystemCertificate.Thumbprint;
            }
        };
    }

    protected HttpSlowPokeClient(Uri uri, Config config, HttpClient? httpClient)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(config);
        Endpoint = uri;
        Config = config;
        HttpClient = httpClient ?? new HttpClient(CreateHandler()) { BaseAddress = uri };
    }

    public Uri Endpoint { get; }
    public Config Config { get; }
    public HttpClient HttpClient { get; }

    public static async Task<ISlowPokeClient> CreateClient(Uri url, Config config, HttpClient? httpClient = null, bool requireConnection = true, CancellationToken cancellationToken = default)
    {
        var client = new HttpSlowPokeClient(url, config, httpClient);
        if (requireConnection)
        {
            await client.Connect(cancellationToken);
        }
        return client;
    }

    public async Task<T?> Query<T>(string path, Action<HttpRequestMessage>? configureRequest, Func<string, Task<T?>> responseHandler, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        configureRequest?.Invoke(request);
        var response = await HttpClient.SendAsync(request, cancellationToken);
        var sr = new StreamReader(response.Content.ReadAsStream(), System.Text.Encoding.UTF8, true, 1024, true);
        var str = sr.ReadToEnd();
        if (response.IsSuccessStatusCode)
        {
            return await responseHandler(str);
        }
        else
        {
            throw new Exception($"Received response {response.StatusCode} from route {response.RequestMessage!.RequestUri}: {str}");
        }
    }

    public void Dispose()
    {
        HttpClient.Dispose();
    }

    public async Task Publish(IBroadcastMessage message, CancellationToken cancellationToken)
    {
        using var serializedMsg = new MemoryStream();
        serializedMsg.Write(BitConverter.GetBytes(1));
        (message.ConvertToRaw() as BroadcastMessageRaw)!.Serialize(serializedMsg);
        serializedMsg.Position = 0;

        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/broadcast");
        var content = new StreamContent(serializedMsg);
        content.Headers.Add("Content-Type", "application/octet-stream");
        msg.Content = content;
        using var response = await HttpClient.SendAsync(msg, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Could not send broadcast messages from {Endpoint}: {response.StatusCode}");
        }
    }

    public async Task<IEnumerable<IBroadcastMessage>> Receive(Guid lastEventReceived, CancellationToken cancellationToken)
    {
        using var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/broadcast"), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Could not receive broadcast messages from {Endpoint}: {response.StatusCode}");
        }
        using var stream = response.Content.ReadAsStream();
        var numToReadBytes = new byte[4];
        stream.Read(numToReadBytes);
        var numToRead = BitConverter.ToInt32(numToReadBytes);
        var list = new List<IBroadcastMessage>();
        if (numToRead > 0)
        {
            while (stream.CanRead && numToRead > list.Count && !cancellationToken.IsCancellationRequested)
            {
                list.Add(BroadcastMessageRaw.Deserialize(stream));
            }
        }
        if (numToRead < list.Count)
        {
            throw new Exception("Length mismatch");
        }
        return list;
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        using var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/ping"), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Could not connect to {Endpoint}: {response.StatusCode}");
        }
    }

    public async Task<int> SearchCount(string path, QueryDocumentOptions? options, CancellationToken cancellationToken)
    {
        return await Search<int>(path, options, null, responseHandler: str => Task.FromResult(int.Parse(str)), cancellationToken);
    }

    public async Task<T?> Search<T>(string path, object? json, Action<HttpRequestMessage>? configureRequest, Func<string, Task<T?>> responseHandler, CancellationToken cancellationToken)
    {
        return (await Query(path, request =>
        {
            request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(json));
            request.Headers.Add("Content-Type", "application/json");
            request.Headers.Add("Accept", "application/json");
            configureRequest?.Invoke(request);
        }, responseHandler, cancellationToken))!;
    }

    public async Task<bool> Ping(CancellationToken cancellationToken)
    {
        return await Query<bool>("api/ping", null, str => Task.FromResult(bool.TryParse(str, out var b) && b), cancellationToken);
    }

    public async Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(node);
        return await Query<bool>($"api/has-meta/{Uri.EscapeDataString(node.Path.PathValue)}", null, str => Task.FromResult(bool.Parse(str)), cancellationToken);
    }

    public async Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(node);
        return (await Query<IReadOnlyDocumentMeta>($"api/get-meta/{Uri.EscapeDataString(node.Path.PathValue)}", null, json => throw new NotImplementedException(json), cancellationToken))!;
    }

    public async Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(path);
        return await Query<bool>($"api/node-exists-at-path/{Uri.EscapeDataString(path.PathValue)}", null, str => Task.FromResult(bool.Parse(str)), cancellationToken);
    }

    private class GraphQLResult<T>
    {
        public T? data { get; set; }
    }

    public async Task<T?> GraphQLQuery<T>(string query, CancellationToken cancellationToken)
    {
        return await Query<T>("api/graphql", request =>
        {
            request.Method = HttpMethod.Post;
            var q = new JsonObject { { nameof(query), "query { " + query + " }" } };
            var c = new StringContent(q.ToString());
            c.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = c;
            request.Headers.Add("Accept", "application/json");
        }, json => Task.FromResult(json != null ? JsonSerializer.Deserialize<GraphQLResult<T>>(json).data : default), cancellationToken);
    }

    public async Task<bool> Sync(bool asynchronous = true, bool immediately = false)
    {
        var args = new List<string>();
        if (asynchronous == false)
        {
            args.Add(nameof(asynchronous) + "=false");
        }
        if (immediately == true)
        {
            args.Add(nameof(immediately) + "=true");
        }

        var argsStr = string.Join("&", args);
        if (argsStr.Length > 0)
        {
            argsStr = "?" + argsStr;
        }
        
        return await Query<bool>($"api/sync{argsStr}", null, str => Task.FromResult(bool.Parse(str)), CancellationToken.None);
    }

    public async Task<bool> IsUpToDate() => await GetSyncState(CancellationToken.None) == SyncState.UpToDate;

    public Task<SyncState> GetSyncState(CancellationToken cancellationToken)
    {
        return Query<SyncState>($"api/sync-state", null, str => Task.FromResult(int.TryParse(str, out var id) ? (SyncState)id : Enum.Parse<SyncState>(str)), cancellationToken);
    }

    public async Task<bool> AddTrusted(string hostUrl, bool testConnection, CancellationToken cancellationToken)
    {
        var path = testConnection ?  "api/add-trusted-test-connection" : "api/add-trusted";
        return await Query<bool>($"{path}/{Uri.EscapeDataString(hostUrl)}", null, str => Task.FromResult(bool.TryParse(str, out var b) && b ? b : throw new Exception(str)), cancellationToken);
    }
}