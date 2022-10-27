using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;
using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Client;


public class HttpSlowPokeClient : ISlowPokeClient
{
    public static X509Certificate2 SystemCertificate { get; set; }

    public HttpSlowPokeClient(Uri uri, Config config)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(config);
        Endpoint = uri;
        Config = config;

        var handler = new HttpClientHandler();

        handler.ServerCertificateCustomValidationCallback = 
            (request, serverCertificate, serverChain, sslPolicyErrors) => {
                return true;// serverCertificate.Thumbprint == SystemCertificate.Thumbprint;
            };

        HttpClient = new HttpClient(handler);
        HttpClient.BaseAddress = uri;
    }

    public Uri Endpoint { get; }
    public Config Config { get; }
    public HttpClient HttpClient { get; }

    public static ISlowPokeClient Connect(string url, Config config, CancellationToken cancellationToken = default)
    {
        var client = new HttpSlowPokeClient(new Uri(url), config);
        client.Connect(cancellationToken);
        return client;
    }

    public T Query<T>(string path, Action<HttpRequestMessage> configureRequest, Func<string, T> responseHandler, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        configureRequest?.Invoke(request);
        var response = HttpClient.Send(request, cancellationToken);
        var sr = new StreamReader(response.Content.ReadAsStream(), System.Text.Encoding.UTF8, true, 1024, true);
        var str = sr.ReadToEnd();
        if (response.IsSuccessStatusCode)
        {
            return responseHandler(str);
        }
        else
        {
            throw new Exception($"Received response {response.StatusCode} from route {path}: {str}");
        }
    }

    public void Dispose()
    {
        HttpClient.Dispose();
    }

    public void Publish(IBroadcastMessage message, CancellationToken cancellationToken)
    {
        using var serializedMsg = new MemoryStream();
        serializedMsg.Write(BitConverter.GetBytes(1));
        (message.ConvertToRaw() as BroadcastMessageRaw)!.Serialize(serializedMsg);
        serializedMsg.Position = 0;

        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/broadcast");
        var content = new StreamContent(serializedMsg);
        content.Headers.Add("Content-Type", "application/octet-stream");
        msg.Content = content;
        using var response = HttpClient.Send(msg, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Could not send broadcast messages from {Endpoint}: {response.StatusCode}");
        }
    }

    public IEnumerable<IBroadcastMessage> Receive(Guid lastEventReceived, CancellationToken cancellationToken)
    {
        using var response = HttpClient.Send(new HttpRequestMessage(HttpMethod.Get, "/api/broadcast"), cancellationToken);
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

    private void Connect(CancellationToken cancellationToken)
    {
        using var response = HttpClient.Send(new HttpRequestMessage(HttpMethod.Get, "/api/ping"), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Could not connect to {Endpoint}: {response.StatusCode}");
        }
    }

    public int SearchCount(string path, QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return Search<int>(path, options, null, responseHandler: str => int.Parse(str), cancellationToken);
    }

    public T Search<T>(string path, object json, Action<HttpRequestMessage> configureRequest, Func<string, T> responseHandler, CancellationToken cancellationToken)
    {
        return Query(path, request =>
        {
            request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(json));
            request.Headers.Add("Content-Type", "application/json");
            request.Headers.Add("Accept", "application/json");
            configureRequest(request);
        }, responseHandler, cancellationToken);
    }

    public bool Ping(CancellationToken cancellationToken)
    {
        return Query<bool>("/ping", null, str => bool.TryParse(str, out var b) && b, cancellationToken);
    }

    public bool HasMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(node);
        return Query<bool>($"api/has-meta/{Uri.EscapeDataString(node.Path.PathValue)}", null, str => bool.Parse(str), cancellationToken);
    }

    public IReadOnlyDocumentMeta GetMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(node);
        return Query<IReadOnlyDocumentMeta>($"api/get-meta/{Uri.EscapeDataString(node.Path.PathValue)}", null, json => throw new NotImplementedException(json), cancellationToken);
    }

    public bool NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(path);
        return Query<bool>($"api/node-exists-at-path/{Uri.EscapeDataString(path.PathValue)}", null, str => bool.Parse(str), cancellationToken);
    }

    private class GraphQLResult<T>
    {
        public T data { get; set; }
    }

    public T GraphQLQuery<T>(string query, CancellationToken cancellationToken)
    {
        return Query<T>("api/graphql", request =>
        {
            request.Method = HttpMethod.Post;
            var q = new JsonObject { { nameof(query), "query { " + query + " }" } };
            var c = new StringContent(q.ToString());
            c.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = c;
            request.Headers.Add("Accept", "application/json");
        }, json => JsonSerializer.Deserialize<GraphQLResult<T>>(json).data, cancellationToken);
    }
}