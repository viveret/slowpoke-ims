using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Services.Node.Docs;


public class UnifiedReadOnlyDocumentProvider : IReadOnlyDocumentResolver
{
    private readonly IDocumentProviderResolver documentProviderResolver;
    private readonly IEnumerable<IReadOnlyDocumentResolver> documentProviders;

    public UnifiedReadOnlyDocumentProvider(
        IDocumentProviderResolver documentProviderResolver,
        IEnumerable<IReadOnlyDocumentResolver> documentProviders)
    {
        this.documentProviderResolver = documentProviderResolver ?? throw new ArgumentNullException(nameof(documentProviderResolver));
        this.documentProviders = documentProviders ?? throw new ArgumentNullException(nameof(documentProviders));
    }

    public string InstanceName => $"{ResolverTypeName} for {HostName}";

    public string HostName => System.Environment.MachineName;

    public virtual string ResolverTypeName => "Unified Read Only";

    public virtual Task<NodePermissionCategories<bool>> Permissions
    {
        get
        {
            return Task.FromResult(new NodePermissionCategories<bool>
            {
                CanRead = true,
            });
        }
    }

    public Task<ISlowPokeHost> Host => throw new NotImplementedException();

    public Task<bool> CanSync => Task.FromResult(false);

    private async Task<TReturn> ForEachProviderCapture<T, TReturn>(Func<T, Task<TReturn>> foreachFn, Func<TReturn, bool> returnValidFn, CancellationToken cancellationToken, TReturn defaultConst = default) where T: IReadOnlyDocumentResolver
    {
        foreach (var provider in documentProviders.OfType<T>())
        {
            if (cancellationToken.IsCancellationRequested)
                break;
                
            var ret = await foreachFn(provider);
            if (returnValidFn(ret))
            {
                return ret;
            }
        }
        return defaultConst;
    }

    public Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        return ForEachProviderCapture<IReadOnlyDocumentResolver, bool>(p => p.NodeExistsAtPath(path, cancellationToken), v => v, cancellationToken);
    }

    public Task<string> GetContentTypeFromExtension(string extension)
    {
        return ForEachProviderCapture<IReadOnlyDocumentResolver, string>(p => p.GetContentTypeFromExtension(extension), _ => true, CancellationToken.None, string.Empty);
    }

    public Task<int> GetCountOfNodes(CancellationToken cancellationToken)
    {
        // this doesn't make sense, should be renamed FirstOrDefaultProviderCapture, use foreach to sum count
        return ForEachProviderCapture<IReadOnlyDocumentResolver, int>(p => p.GetCountOfNodes(cancellationToken), amt => amt >= 0, cancellationToken, -1);
    }

    public Task<int> GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        return ForEachProviderCapture<IReadOnlyDocumentResolver, int>(p => p.GetCountOfNodesInFolder(folder, cancellationToken), amt => amt > 0, cancellationToken, 0);
    }

    public Task<int> GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return ForEachProviderCapture<IReadOnlyDocumentResolver, int>(p => p.GetCountOfNodes(options, cancellationToken), amt => amt > 0, cancellationToken, 0);
    }

    public Task<IReadOnlyNode> GetNodeAtPath(INodePath path, CancellationToken cancellationToken)
    {
        return ForEachProviderCapture<IReadOnlyDocumentResolver, IReadOnlyNode>(p => p.GetNodeAtPath(path, cancellationToken), doc => doc != null, cancellationToken);
    }
    
    public Task<IEnumerable<IReadOnlyNode>> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken)
    {
        // this could also be an aggregate
        return ForEachProviderCapture<IReadOnlyDocumentResolver, IEnumerable<IReadOnlyNode>>(p => p.GetNodesInFolder(folder, offset, amount, cancellationToken), docs => docs != null && docs.Any(), cancellationToken, defaultConst: Enumerable.Empty<IReadOnlyNode>());
    }

    public Task<string> GetExtensionFromContentType(string contentType)
    {
        return ForEachProviderCapture<IReadOnlyDocumentResolver, string>(p => p.GetExtensionFromContentType(contentType), ext => ext.HasValue(), CancellationToken.None);
    }

    public Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return ForEachProviderCapture<IReadOnlyDocumentResolver, IReadOnlyDocumentMeta>(p => p.GetMeta(node, cancellationToken), node => node != null, cancellationToken);
    }

    public Task<IEnumerable<IReadOnlyNode>> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return ForEachProviderCapture<IReadOnlyDocumentResolver, IEnumerable<IReadOnlyNode>>(p => p.GetNodes(options, cancellationToken), nodes => nodes != null && nodes.Any(), cancellationToken, defaultConst: Enumerable.Empty<IReadOnlyNode>());
    }

    public Task<IEnumerable<INodePath>> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return ForEachProviderCapture<IReadOnlyDocumentResolver, IEnumerable<INodePath>>(p => p.GetPaths(options, cancellationToken), paths => paths != null && paths.Any(), cancellationToken, defaultConst: Enumerable.Empty<INodePath>());
    }

    public Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return ForEachProviderCapture<IReadOnlyDocumentResolver, bool>(p => p.HasMeta(node, cancellationToken), v => v, cancellationToken);
    }

    public Task<int> GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<INodeFingerprint>> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return ForEachProviderCapture<IReadOnlyDocumentResolver, IEnumerable<INodeFingerprint>>(p => p.FetchFingerprintsForNode(node, cancellationToken), prints => prints != null && prints.Any(), cancellationToken, defaultConst: Enumerable.Empty<INodeFingerprint>());
    }
}