using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadOnlyLocal;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;



public class GenericReadOnlyDocumentResolver : IReadOnlyDocumentResolver
{
    protected readonly Config config;
    protected readonly IBroadcastProvider broadcastProvider;
    public readonly InMemoryGenericDocumentRepository inMemoryGenericDocumentRepository;

    public GenericReadOnlyDocumentResolver(
        Config config,
        IBroadcastProviderResolver broadcastProvider,
        InMemoryGenericDocumentRepository inMemoryGenericDocumentRepository)
    {
        this.config = config;
        this.broadcastProvider = broadcastProvider.MemCached;
        this.inMemoryGenericDocumentRepository = inMemoryGenericDocumentRepository;
    }

    public string InstanceName => nameof(InstanceName);

    public string ResolverTypeName => nameof(InstanceName);

    public Task<ISlowPokeHost> Host => Task.FromResult<ISlowPokeHost>(new SlowPokeHostModel());

    public Task<bool> CanSync => Task.FromResult(false);

    public virtual Task<NodePermissionCategories<bool>> Permissions => Task.FromResult(new NodePermissionCategories<bool> () { CanRead = true, LimitedToUserOnly = true });

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
        return Task.FromResult(inMemoryGenericDocumentRepository.Files.Forward.Count + inMemoryGenericDocumentRepository.Folders.Forward.Count);
    }

    public async Task<int> GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return (await Filter(options, cancellationToken)).Count();
    }

    public async Task<int> GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        return (await Filter(new QueryDocumentOptions {  }, cancellationToken)).Count();
    }

    public Task<int> GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return GetCountOfNodes(options, cancellationToken);
    }

    public Task<string> GetExtensionFromContentType(string contentType)
    {
        return Task.FromResult(string.Empty);
    }

    public virtual Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyDocumentMeta>(new GenericReadOnlyDocumentMeta(this, node.Path));
    }

    public virtual Task<IReadOnlyNode> GetNodeAtPath(INodePath path, CancellationToken cancellationToken)
    {
        if (path.IsDocument)
        {
            return Task.FromResult<IReadOnlyNode>(new GenericReadOnlyDocument(this, broadcastProvider, path));// inMemoryGenericDocumentRepository!.Files[path.PathValue]
        }
        else
        {
            return Task.FromResult<IReadOnlyNode>(new GenericReadOnlyFolder(this, broadcastProvider, path));
            
        }
        //return new GenericReadOnlyDocument(this, , "/".AsIDocPath(config));
    }

    public async Task<IEnumerable<IReadOnlyNode>> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return (await Filter(options, cancellationToken)).Skip(options.Offset).Take(options.PageSize).ToList();
    }

    public async Task<IEnumerable<IReadOnlyNode>> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken)
    {
        return (await Filter(new QueryDocumentOptions {}, cancellationToken)).Skip(offset).Take(amount).ToList();
    }

    public async Task<IEnumerable<INodePath>> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return (await Filter(options, cancellationToken)).Select(n => n.Path).Skip(options.Offset).Take(options.PageSize).ToList();
    }

    private Task<IEnumerable<IReadOnlyNode>> Filter(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return this.inMemoryGenericDocumentRepository.Filter(options, cancellationToken, this, broadcastProvider, config);
    }

    public Task<bool> HasMeta(INodePath path, CancellationToken cancellationToken)
    {
        var p = path.PathValue;
        if (!inMemoryGenericDocumentRepository.Files.Forward.TryGetValue(p, out var guid))
        {
            if (!inMemoryGenericDocumentRepository.Folders.Forward.TryGetValue(p, out guid))
            {
                return Task.FromResult(false);
            }
        }
        return Task.FromResult(inMemoryGenericDocumentRepository.NodeMetaJson.ContainsKey(guid));
    }

    public Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        return Task.FromResult(inMemoryGenericDocumentRepository.Files.Forward.ContainsKey(path.PathValue) ||
                inMemoryGenericDocumentRepository.Folders.Forward.ContainsKey(path.PathValue));
    }

    public Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return Task.FromResult(inMemoryGenericDocumentRepository.TryGetMetaForPath(node.Path.PathValue, out var _));
    }
}