using System.Collections.Concurrent;
using slowpoke.core.Client;
using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Identity;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.SyncState;

namespace SlowPokeIMS.Tests.Core.Client;


public class FakeSlowPokeClient : ISlowPokeClient
{
    private readonly ConcurrentDictionary<string, TrustLevel> trustedHosts = new ();

    public FakeSlowPokeClient(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        Endpoint = uri;
    }

    public Uri Endpoint { get; }

    public Task<bool> AddTrusted(string hostUri, bool testConnection, CancellationToken cancellationToken)
    {
        trustedHosts[hostUri] = TrustLevel.Trusted;
        return Task.FromResult(true);
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
        return Task.FromResult(SyncState.Uninitialized);
    }

    public Task<T?> GraphQLQuery<T>(string query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }

    public Task<bool> IsUpToDate()
    {
        return Task.FromResult(false);
    }

    public Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }

    public Task<bool> Ping(CancellationToken token)
    {
        return Task.FromResult(true);
    }

    public Task Publish(IBroadcastMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<T?> Query<T>(string path, Action<HttpRequestMessage>? configureRequest, Func<string, Task<T?>> responseHandler, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IBroadcastMessage>> Receive(Guid lastEventReceived, CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Empty<IBroadcastMessage>());
    }

    public Task<T?> Search<T>(string path, object json, Action<HttpRequestMessage>? configureRequest, Func<string, Task<T?>> responseHandler, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> SearchCount(string path, QueryDocumentOptions? options, CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }

    public Task<bool> Sync(bool asynchronous = true, bool immediately = false)
    {
        return Task.FromResult(trustedHosts.Count > 0); // cannot sync if no other known hosts
    }
}