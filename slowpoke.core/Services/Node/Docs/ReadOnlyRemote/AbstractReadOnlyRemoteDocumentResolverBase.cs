using slowpoke.core.Extensions;
using slowpoke.core.Models.Config;
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

    public NodePermissionCategories<bool> Permissions
    {
        get
        {
            return new NodePermissionCategories<bool>
            {
                CanRead = true,
                CanWrite = false,
                IsEncrypted = false,
                IsRemote = true,
                UnlimitedUniversalPublicAccess = true,
            };
        }
    }

    public ISlowPokeHost Host => throw new NotImplementedException();

    public bool IsLocalHost => Config.P2P.ServingAddress.Equals(this.Endpoint.ToString()) || "127.0.0.1".Equals(this.Endpoint.ToString()) || "localhost" == this.Endpoint.Host;

    public bool CanSync => false;

    public abstract bool NodeExistsAtPath(INodePath path, CancellationToken cancellationToken);
    public abstract string GetContentTypeFromExtension(string extension);
    public abstract int GetCountOfNodes(CancellationToken cancellationToken);
    public abstract int GetCountOfDocuments(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract int GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken);
    public abstract int GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract IReadOnlyNode GetNodeAtPath(INodePath path, CancellationToken cancellationToken);
    public abstract IEnumerable<IReadOnlyDocument> GetDocuments(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract IEnumerable<IReadOnlyNode> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken);
    public abstract string GetExtensionFromContentType(string contentType);
    // public abstract IEnumerable<IReadOnlyDocument> GetLeastRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken);
    // public abstract IEnumerable<IReadOnlyDocument> GetLeastRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken);
    public abstract IReadOnlyDocumentMeta GetMeta(IReadOnlyNode node, CancellationToken cancellationToken);
    // public abstract IEnumerable<IReadOnlyDocument> GetMostRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken);
    // public abstract IEnumerable<IReadOnlyDocument> GetMostRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken);
    public abstract IEnumerable<IReadOnlyNode> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract IEnumerable<INodePath> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract int GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken);
    public abstract bool HasMeta(IReadOnlyNode node, CancellationToken cancellationToken);
    public abstract IEnumerable<INodeFingerprint> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken);
}