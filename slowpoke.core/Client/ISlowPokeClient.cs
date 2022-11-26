using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Identity;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.SyncState;
using slowpoke.core.Services.Broadcast;

namespace slowpoke.core.Client;


public interface ISlowPokeClient : IDisposable, IBroadcastProvider
{
    Uri Endpoint { get; }
    Task<ISlowPokeIdentity> GetIdentity(CancellationToken cancellationToken);
    Task<ISlowPokeHost> GetDetails(CancellationToken cancellationToken);

    Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken);
    Task<bool> Ping(CancellationToken token);
    Task<int> SearchCount(string path, QueryDocumentOptions? options, CancellationToken cancellationToken);
    Task<T?> Search<T>(string path, object json, Action<HttpRequestMessage>? configureRequest, Func<string, Task<T?>> responseHandler, CancellationToken cancellationToken);
    Task<T?> Query<T>(string path, Action<HttpRequestMessage>? configureRequest, Func<string, Task<T?>> responseHandler, CancellationToken cancellationToken);
    Task<T?> GraphQLQuery<T>(string query, CancellationToken cancellationToken);
    Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken);
    Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken);
    Task<bool> Sync(bool asynchronous = true, bool immediately = false);
    Task<SyncState> GetSyncState(CancellationToken cancellationToken);
    Task<bool> IsUpToDate();


    // Admin / actions
    Task<bool> AddTrusted(string hostUri, bool testConnection, CancellationToken cancellationToken);
}