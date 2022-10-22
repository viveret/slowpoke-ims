using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Broadcast;

namespace slowpoke.core.Client;


public interface ISlowPokeClient : IDisposable, IBroadcastProvider
{
    Uri Endpoint { get; }
    
    bool NodeExistsAtPath(INodePath path, CancellationToken cancellationToken);
    
    bool Ping(CancellationToken token);

    int SearchCount(string path, QueryDocumentOptions options, CancellationToken cancellationToken);

    T Search<T>(string path, object json, Action<HttpRequestMessage> configureRequest, Func<string, T> responseHandler, CancellationToken cancellationToken);

    T Query<T>(string path, Action<HttpRequestMessage> configureRequest, Func<string, T> responseHandler, CancellationToken cancellationToken);
    
    bool HasMeta(IReadOnlyNode node, CancellationToken cancellationToken);

    IReadOnlyDocumentMeta GetMeta(IReadOnlyNode node, CancellationToken cancellationToken);
}