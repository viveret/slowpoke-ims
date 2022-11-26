using slowpoke.core.Extensions;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Services.Node.Docs;



public abstract class AbstractReadOnlyRemoteDocumentResolverBase: IReadOnlyRemoteDocumentResolver
{
    public Uri Endpoint { get; }
    public IDocumentProviderResolver DocumentProviderResolver { get; }
    public Config Config { get; }

    protected AbstractReadOnlyRemoteDocumentResolverBase(Uri endpoint, IDocumentProviderResolver documentProviderResolver, Config config)
    {
        Endpoint = endpoint;
        DocumentProviderResolver = documentProviderResolver;
    }

    public string InstanceName => $"{ResolverTypeName} at {Endpoint}";

    public string HostName => Endpoint.Host;

    public virtual string ResolverTypeName => $"Read Only Remote ({this.GetType().Name.TrimEnd("DocumentProvider")})";

    public virtual Task<NodePermissionCategories<bool>> Permissions
    {
        get
        {
            return Task.FromResult(new NodePermissionCategories<bool>
            {
                CanRead = true,
                CanWrite = false,
                IsEncrypted = false,
                IsRemote = true,
                UnlimitedUniversalPublicAccess = true,
            });
        }
    }

    public Task<ISlowPokeHost> Host => throw new NotImplementedException();

    public bool IsLocalHost => Config.P2P.ServingAddress.Equals(this.Endpoint.ToString()) || "127.0.0.1".Equals(this.Endpoint.ToString()) || "localhost" == this.Endpoint.Host;

    public Task<bool> CanSync => Task.FromResult(false);

    public abstract Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken);
    public abstract Task<string> GetContentTypeFromExtension(string extension);
    public abstract Task<int> GetCountOfNodes(CancellationToken cancellationToken);
    public abstract Task<int> GetCountOfDocuments(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract Task<int> GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken);
    public abstract Task<int> GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract Task<IReadOnlyNode> GetNodeAtPath(INodePath path, CancellationToken cancellationToken);
    public abstract Task<IEnumerable<IReadOnlyDocument>> GetDocuments(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract Task<IEnumerable<IReadOnlyNode>> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken);
    public abstract Task<string> GetExtensionFromContentType(string contentType);
    // public Task<abstract> IEnumerable<IReadOnlyDocument> GetLeastRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken);
    // public Task<abstract> IEnumerable<IReadOnlyDocument> GetLeastRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken);
    public abstract Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken);
    // public Task<abstract> IEnumerable<IReadOnlyDocument> GetMostRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken);
    // public Task<abstract> IEnumerable<IReadOnlyDocument> GetMostRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken);
    public abstract Task<IEnumerable<IReadOnlyNode>> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract Task<IEnumerable<INodePath>> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract Task<int> GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken);
    public abstract Task<IEnumerable<INodeFingerprint>> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken);
}