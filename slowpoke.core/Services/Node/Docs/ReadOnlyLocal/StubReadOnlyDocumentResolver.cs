using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadOnlyLocal;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;



public class StubReadOnlyDocumentResolver : IReadOnlyDocumentResolver
{
    private readonly Config config;

    public StubReadOnlyDocumentResolver(Config config)
    {
        this.config = config;
    }

    public string InstanceName => nameof(InstanceName);

    public string ResolverTypeName => nameof(InstanceName);

    public Task<ISlowPokeHost> Host => Task.FromResult<ISlowPokeHost>(new SlowPokeHostModel());

    public Task<bool> CanSync => Task.FromResult(false);

    public Task<NodePermissionCategories<bool>> Permissions => Task.FromResult(new NodePermissionCategories<bool>());

    public Task<IEnumerable<INodeFingerprint>> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Empty<INodeFingerprint>());
    }

    public Task<string> GetContentTypeFromExtension(string extension)
    {
        return Task.FromResult(string.Empty);
    }

    public Task<int> GetCountOfNodes(CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }

    public Task<int> GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }

    public Task<int> GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }

    public Task<int> GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }

    public Task<string> GetExtensionFromContentType(string contentType)
    {
        return Task.FromResult(string.Empty);
    }

    public Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyDocumentMeta>(new StubReadOnlyDocumentMeta(config, "/".AsIDocPath(config)));
    }

    public Task<IReadOnlyNode> GetNodeAtPath(INodePath path, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyNode>(new StubReadOnlyDocument(true, "/".AsIDocPath(config), config));
    }

    public Task<IEnumerable<IReadOnlyNode>> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Empty<IReadOnlyNode>());
    }

    public Task<IEnumerable<IReadOnlyNode>> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Empty<IReadOnlyNode>());
    }

    public Task<IEnumerable<INodePath>> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Empty<INodePath>());
    }

    public Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }

    public Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}