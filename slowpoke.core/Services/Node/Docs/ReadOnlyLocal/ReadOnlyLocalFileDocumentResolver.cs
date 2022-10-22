using System.Linq;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadOnlyLocal;
using slowpoke.core.Services.Broadcast;

namespace slowpoke.core.Services.Node.Docs;

public class ReadOnlyLocalDocumentResolver : IReadOnlyDocumentResolver
{
    protected readonly Config config;
    protected readonly IBroadcastProviderResolver broadcastProviderResolver;

    public ReadOnlyLocalDocumentResolver(Config config, IBroadcastProviderResolver broadcastProviderResolver)
    {
        this.config = config;
        this.broadcastProviderResolver = broadcastProviderResolver;
    }

    public string InstanceName => System.Environment.MachineName;

    public string HostName => System.Environment.UserDomainName ?? System.Environment.MachineName;

    public string ResolverTypeName => "Read Only Local";
    
    public bool CanSync => config.P2P.SyncEnabled;

    public virtual NodePermissionCategories<bool> Permissions
    {
        get
        {
            return new NodePermissionCategories<bool>
            {
                CanRead = true,
                CanWrite = false,
                IsEncrypted = false,
                LimitedToUserOnly = false,
                LimitedToMachineOnly = true,
                LimitedToLocalNetworkOnly = false,
                LimitedToAllowedConnectionsOnly = false,
                UnlimitedUniversalPublicAccess = false,
            };
        }
    }

    public ISlowPokeHost Host => new SlowPokeHostModel { Label = "localhost" };

    public int GetCountOfNodes(CancellationToken cancellationToken) => Directory.GetFileSystemEntries(config.Paths.HomePath, Config.PathsConfig.DocMetaExtensionPattern, new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true }).Count();

    public int GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken) => Directory.GetFileSystemEntries(folder.ConvertToAbsolutePath().PathValue, Config.PathsConfig.DocMetaExtensionPattern, new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = false }).Count();

    public virtual IReadOnlyDocumentMeta GetMeta(IReadOnlyNode node, CancellationToken cancellationToken) => new ReadOnlyDocumentMeta(this, node.Path);

    public bool HasMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        var metaPath = node.Path.ConvertToAbsolutePath().ConvertToMetaPath().PathValue;
        return File.Exists(metaPath);
    }

    public virtual IReadOnlyNode GetNodeAtPath(INodePath path, CancellationToken cancellationToken) => new ReadOnlyDocument(this, broadcastProviderResolver.MemCached, path);
    
    public virtual IReadOnlyFolder GetFolderAtPath(INodePath path, CancellationToken cancellationToken) => new ReadOnlyFolder(this, broadcastProviderResolver.MemCached, path);

    public IEnumerable<IReadOnlyNode> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken) => System.IO.Directory.EnumerateFiles(folder.ConvertToAbsolutePath().PathValue, Config.PathsConfig.DocMetaExtensionPattern)
                                                                                                            .Skip(offset).Take(amount).Select(p => GetNodeAtPath(p.AsIDocPath(config), cancellationToken));

    public int GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken) => GetEnumerableNodes(options, cancellationToken).Count();

    public IEnumerable<IReadOnlyNode> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        var nodes = GetEnumerableNodes(options, cancellationToken);
        switch (options.OrderByColumn)
        {
            case nameof(IReadOnlyDocumentMeta.LastUpdate):
            nodes = options.OrderByAscending ? nodes.OrderBy(f => f.Meta.LastUpdate): nodes.OrderByDescending(f => f.Meta.LastUpdate);
            break;
            case nameof(IReadOnlyDocumentMeta.CreationDate):
            nodes = options.OrderByAscending ? nodes.OrderBy(f => f.Meta.CreationDate): nodes.OrderByDescending(f => f.Meta.CreationDate);
            break;
            case nameof(IReadOnlyDocumentMeta.ArchivedDate):
            nodes = options.OrderByAscending ? nodes.OrderBy(f => f.Meta.ArchivedDate): nodes.OrderByDescending(f => f.Meta.ArchivedDate);
            break;
            case nameof(IReadOnlyDocumentMeta.LastSyncDate):
            nodes = options.OrderByAscending ? nodes.OrderBy(f => f.Meta.LastSyncDate): nodes.OrderByDescending(f => f.Meta.LastSyncDate);
            break;
            case nameof(IReadOnlyDocumentMeta.Title):
            {
                IComparer<string> comparer = new NodeTitleComparer();
                nodes = options.OrderByAscending ? nodes.OrderBy(k => k.Meta.Title, comparer: comparer) : nodes.OrderByDescending(k => k.Meta.Title, comparer: comparer);
                break;
            }
            case nameof(IReadOnlyDocument.ContentType):
            nodes = nodes.OrderBy(k => k.Meta.ContentType);
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

    private IEnumerable<IReadOnlyNode> GetEnumerableNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return GetPaths(options, cancellationToken)
                        .Select(p => p.IsDocument ? (IReadOnlyNode) GetNodeAtPath(p, cancellationToken) : GetFolderAtPath(p, cancellationToken));
    }

    public int GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        return GetPaths(options, cancellationToken).Count();
    }

    public IEnumerable<INodePath> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        var path = string.Empty;
        path = options.Path.HasValue() ? Path.Combine(path, options.Path.ConvertToAbsolutePath().PathValue) : path;
        path = options.Folder.HasValue() ? Path.Combine(path, options.Folder.PathValue): path;
        
        var ext = options.Extension;
        var hasExt = !string.IsNullOrWhiteSpace(ext);

        if (Directory.Exists(path))
        {
            var enumOptions = new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = options.Recursive };
            var items = options.IncludeFolders ?
                            (hasExt ? Directory.GetFileSystemEntries(path, ext, enumOptions) : Directory.GetFileSystemEntries(path, "*", enumOptions)) :
                            (hasExt ? Directory.GetFiles(path, ext, enumOptions) : Directory.GetFiles(path, "*", enumOptions));
            var paths = items
                .Where(p => !config.Paths.IsOS(p));
            var pathCategories = paths.Select(p => (p, p.ToLower().Split('/')))
                                        .ToDictionary(k => k.p,
                                                      p =>
                                                      {
                                                            var categories = config.Paths.FolderToCategoryMap
                                                                .Where(folder => p.Item2.Contains(folder.Key))
                                                                .Select(folder => folder.Value)
                                                                .ToList();
                                                            if (config.Paths.ExtensionToCategoryMap.TryGetValue(p.p.GetFullExtension(), out var cat))
                                                            {
                                                                categories.Add(cat);
                                                            }
                                                            return categories;
                                                      });
            if (!options.IncludeHidden)
            {
                paths = paths.Where(p =>
                {
                    if (System.IO.Path.HasExtension(p))
                    {
                        return !System.IO.Path.GetFileName(p).StartsWith('.');
                    }
                    else
                    {
                        var dir = System.IO.Path.GetDirectoryName(p);
                        return dir != null ? !dir.StartsWith('.') : !p.StartsWith("/.");
                    }
                });
            }
            if (options.CategoriesToIncludeOnly != null && options.CategoriesToIncludeOnly.Length > 0)
            {
                paths = paths
                    .Where(p => 
                    {
                        if (!System.IO.File.Exists(p))
                            return true; // skip non-files

                        var pc = pathCategories[p];
                        return pc.Count == options.CategoriesToIncludeOnly.Length &&
                            pc.Intersect(options.CategoriesToIncludeOnly).Count() == options.CategoriesToIncludeOnly.Length;
                    });
            }
            if (options.CategoriesToExclude != null && options.CategoriesToExclude.Length > 0)
            {
                paths = paths
                    .Where(p => pathCategories[p].Intersect(options.CategoriesToExclude).Any());
            }
            if (options.ContentType.HasValue())
            {
                paths = paths.Where(p => options.ContentType == GetContentTypeFromExtension(p.GetFullExtension()));
            }
            if (options.SyncEnabled.HasValue)
            {
                paths = paths.Where(p => new ReadOnlyDocumentMeta(this, p.AsIDocPath(config).ConvertToMetaPath()).SyncEnabled == options.SyncEnabled.Value);
            }
            if (options.IsInFavorites.HasValue)
            {
                paths = paths.Where(p => new ReadOnlyDocumentMeta(this, p.AsIDocPath(config).ConvertToMetaPath()).Favorited == options.IsInFavorites.Value);
            }
            return paths.Select(p => p.AsIDocPath(config).ConvertToAbsolutePath());
        }
        else
        {
            return Enumerable.Empty<INodePath>();
        }
    }

    public bool NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        return File.Exists(path.ConvertToAbsolutePath().PathValue);
    }

    public string GetContentTypeFromExtension(string extension)
    {
        if (extension.HasValue())
        {
            if (extension.StartsWith('.'))
            {
                extension = extension.Substring(1);
            }
            var matches = config.Paths.ContentTypeToExtensionMap.Where(kvp => kvp.Value == extension);
            return matches.Select(kvp => kvp.Key).FirstOrDefault() ?? string.Empty;
        }
        else
        {
            return string.Empty;
        }
    }

    public string GetExtensionFromContentType(string contentType)
    {
        return contentType.HasValue() && config.Paths.ContentTypeToExtensionMap.TryGetValue(contentType.ToLower(), out var ext) ? ext : string.Empty;
    }

    public IEnumerable<INodeDiffBrief> FetchChangesForNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public IEnumerable<INodeFingerprint> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        yield return node.GetFingerprint(cancellationToken);
    }
}