using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Identity;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.SyncState;

namespace slowpoke.core.Client;


public class StubSlowPokeClient : ISlowPokeClient
{
    public StubSlowPokeClient(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        Endpoint = uri;
    }

    public Uri Endpoint { get; }

    public Task<bool> AddTrusted(string hostUri, bool testConnection, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose() { }

    public Task<ISlowPokeHost> GetDetails(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ISlowPokeIdentity> GetIdentity(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<SyncState> GetSyncState(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GraphQLQuery<T>(string query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsUpToDate()
    {
        throw new NotImplementedException();
    }

    public Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Ping(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task Publish(IBroadcastMessage message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<T?> Query<T>(string path, Action<HttpRequestMessage>? configureRequest, Func<string, Task<T?>> responseHandler, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IBroadcastMessage>> Receive(Guid lastEventReceived, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<T?> Search<T>(string path, object json, Action<HttpRequestMessage>? configureRequest, Func<string, Task<T?>> responseHandler, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> SearchCount(string path, QueryDocumentOptions? options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Sync(bool asynchronous = true, bool immediately = false)
    {
        throw new NotImplementedException();
    }
}