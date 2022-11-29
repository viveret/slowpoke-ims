using System.Linq;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadOnlyLocal;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Util;
using SlowPokeIMS.Core.Collections;
using SlowPokeIMS.Core.Services.Node.Docs.PathRules;

namespace slowpoke.core.Services.Node.Docs;

public class ReadOnlyLocalDocumentResolver : IReadOnlyDocumentResolver
{
    protected readonly Config config;
    protected readonly IBroadcastProviderResolver broadcastProviderResolver;
    private readonly IPathRuleService pathRuleService;

    public ReadOnlyLocalDocumentResolver(
        Config config,
        IBroadcastProviderResolver broadcastProviderResolver,
        IPathRuleService pathRuleService)
    {
        this.config = config;
        this.broadcastProviderResolver = broadcastProviderResolver;
        this.pathRuleService = pathRuleService;
    }

    public string InstanceName => System.Environment.MachineName;

    public string HostName => System.Environment.UserDomainName ?? System.Environment.MachineName;

    public string ResolverTypeName => "Read Only Local";
    
    public Task<bool> CanSync => Task.FromResult(config.P2P.SyncEnabled);

    public virtual Task<NodePermissionCategories<bool>> Permissions
    {
        get
        {
            return Task.FromResult(new NodePermissionCategories<bool>
            {
                CanRead = true,
                CanWrite = false,
                IsEncrypted = false,
                LimitedToUserOnly = false,
                LimitedToMachineOnly = true,
                LimitedToLocalNetworkOnly = false,
                LimitedToAllowedConnectionsOnly = false,
                UnlimitedUniversalPublicAccess = false,
            });
        }
    }

    public Task<ISlowPokeHost> Host => Task.FromResult<ISlowPokeHost>(new SlowPokeHostModel { Label = "localhost" });

    public Task<int> GetCountOfNodes(CancellationToken cancellationToken) => Task.FromResult(
        Directory.EnumerateFileSystemEntries(
            config.Paths.HomePath,
            Config.PathsConfig.DocMetaExtensionPattern,
            new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true }
        ).Count()
    );

    public Task<int> GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken) => Task.FromResult(
        Directory.EnumerateFileSystemEntries(
            folder.ConvertToAbsolutePath().PathValue,
            Config.PathsConfig.DocMetaExtensionPattern,
            new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = false }
        ).Count()
    );

    public virtual Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyDocumentMeta>(
        new ReadOnlyDocumentMeta(this, node.Path.ConvertToMetaPath())
    );

    public Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        var metaPath = node.Path.ConvertToAbsolutePath().ConvertToMetaPath().PathValue;
        return Task.FromResult(File.Exists(metaPath));
    }

    public virtual Task<IReadOnlyNode> GetNodeAtPath(INodePath path, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyNode>(
        path.IsDocument && !path.IsMeta ?
            new ReadOnlyDocument(this, broadcastProviderResolver.MemCached, path)
            :
            new ReadOnlyFolder(this, broadcastProviderResolver.MemCached, path)
    );
    
    // public virtual Task<IReadOnlyFolder> GetFolderAtPath(INodePath path, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyFolder>(
    //     new ReadOnlyFolder(this, broadcastProviderResolver.MemCached, path)
    // );

    public async Task<IEnumerable<IReadOnlyNode>> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken) => await Task.WhenAll(
        System.IO.Directory.EnumerateFiles(folder.ConvertToAbsolutePath().PathValue, Config.PathsConfig.DocMetaExtensionPattern)
            .Skip(offset).Take(amount).Select(p => GetNodeAtPath(p.AsIDocPath(config), cancellationToken))
    );

    public async Task<int> GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken) => (await GetEnumerableNodes(options, cancellationToken)).Count();

    public async Task<IEnumerable<IReadOnlyNode>> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        var nodes = await GetEnumerableNodes(options, cancellationToken);
        switch (options.OrderByColumn)
        {
            case nameof(IReadOnlyDocumentMeta.LastUpdate):
            nodes = await (options.OrderByAscending ? nodes.OrderByAsync(async f => (await f.Meta).LastUpdate): nodes.OrderByDescendingAsync(async f => (await f.Meta).LastUpdate));
            break;
            case nameof(IReadOnlyDocumentMeta.CreationDate):
            nodes = await (options.OrderByAscending ? nodes.OrderByAsync(async f => (await f.Meta).CreationDate): nodes.OrderByDescendingAsync(async f => (await f.Meta).CreationDate));
            break;
            case nameof(IReadOnlyDocumentMeta.ArchivedDate):
            nodes = await (options.OrderByAscending ? nodes.OrderByAsync(async f => (await f.Meta).ArchivedDate): nodes.OrderByDescendingAsync(async f => (await f.Meta).ArchivedDate));
            break;
            case nameof(IReadOnlyDocumentMeta.LastSyncDate):
            nodes = await (options.OrderByAscending ? nodes.OrderByAsync(async f => (await f.Meta).LastSyncDate): nodes.OrderByDescendingAsync(async f => (await f.Meta).LastSyncDate));
            break;
            case nameof(IReadOnlyDocumentMeta.Title):
            {
                IComparer<string> comparer = new NodeTitleComparer();
                nodes = await (options.OrderByAscending ? nodes.OrderByAsync(async k => (await k.Meta).Title, comparer: comparer) : nodes.OrderByDescendingAsync(async k => (await k.Meta).Title, comparer: comparer));
                break;
            }
            case "ContentType":
            nodes = await nodes.OrderByAsync(async k => (await k.Meta).ContentType);
            break;
            case nameof(IReadOnlyDocument.Path):
            {
                IComparer<INodePath> comparer = options.Recursive ? new NodePathComparer() : new NodePathFoldersFirstComparer();
                nodes = nodes.OrderBy(k => k.Path, comparer: comparer);
                break;
            }
            case nameof(IReadOnlyDocument.SizeBytes):
            nodes = options.OrderByAscending ? nodes.OrderBy(f => f.SizeBytes): nodes.OrderByDescending(f => f.SizeBytes);
            break;
            case "Extension":
            nodes = options.OrderByAscending ? nodes.OrderBy(f => f.Path.IsDocument ? f.Path.PathValue.GetFullExtension() : string.Empty): nodes.OrderByDescending(f => f.SizeBytes);
            break;
        }
        return nodes.Skip(options.Offset).Take(options.PageSize).ToList();
    }

    private Task<IEnumerable<IReadOnlyNode>> GetEnumerableNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return SearchLocalForNodes(options, cancellationToken);
        // need this to map correct types to instantiate
        //var docs = await Task.WhenAll(nodes.Where(p => p.Path.IsDocument && !p.Path.IsMeta).Select(p => GetNodeAtPath(p, cancellationToken)));
        //var folders = await Task.WhenAll(nodes.Where(p => p.Path.IsFolder && !p.Path.IsMeta).Select(p => GetFolderAtPath(p, cancellationToken)));
        //return docs.Concat(folders);
    }

    public async Task<int> GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return (await SearchLocalForNodes(options, cancellationToken)).Count();
    }

    // todo: need to take into consideration ignore paths, like if a folder contains a .git folder it should be excluded.
    // also need to fix for other rules, and make rule system extendable and scriptable / configurable
    // similar to gitignore
    private async Task<IEnumerable<IReadOnlyNode>> SearchLocalForNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        var path = string.Empty;
        path = options.Path.HasValue() ? Path.Combine(path, options.Path!.ConvertToAbsolutePath().PathValue) : path;
        path = options.Folder.HasValue() ? Path.Combine(path, options.Folder!.PathValue): path;
        
        var ext = options.Extension ?? string.Empty;

        if (Directory.Exists(path))
        {
            var enumOptions = new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = options.Recursive, ReturnSpecialDirectories = false, MaxRecursionDepth = 10 };
            var paths = options.IncludeFolders ?
                            DirectoryExtensions.EnumerateEntriesSafe(path, ext, enumOptions) :
                            DirectoryExtensions.EnumerateFilesSafe(path, ext, enumOptions);
            var nodes = await pathRuleService.FilterAndResolvePaths(paths, options, this, cancellationToken);
            return nodes;
        }
        else
        {
            return Enumerable.Empty<IReadOnlyNode>();
        }
    }

    public Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        return Task.FromResult(File.Exists(path.ConvertToAbsolutePath().PathValue));
    }


    public Task<string> GetExtensionFromContentType(string contentType)
    {
        return Task.FromResult(contentType.HasValue() && config.Paths.ContentTypeToExtensionMap.TryGetValue(contentType.ToLower(), out var ext) ? ext : string.Empty);
    }

    public Task<IEnumerable<INodeDiffBrief>> FetchChangesForNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public async Task<IEnumerable<INodeFingerprint>> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return new INodeFingerprint[] { await node.GetFingerprint(cancellationToken) };
    }

    public Task<string> GetContentTypeFromExtension(string extension)
    {
        if (extension.HasValue())
        {
            if (extension.StartsWith('.'))
            {
                extension = extension.Substring(1);
            }
            var matches = config.Paths.ContentTypeToExtensionMap.Where(kvp => kvp.Value == extension);
            return Task.FromResult(matches.Select(kvp => kvp.Key).FirstOrDefault() ?? string.Empty);
        }
        else
        {
            return Task.FromResult(string.Empty);
        }
    }
}