using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using slowpoke.core;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadOnlyLocal;
using slowpoke.core.Services.Broadcast;
using SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;

namespace SlowPokeIMS.Core.Services.Node.Docs;



public class InMemoryGenericDocumentRepository
{
    public BiDirectional<string, Guid, ConcurrentDictionary<string, Guid>, ConcurrentDictionary<Guid, string>> Files { get; } = new();
    public BiDirectional<string, Guid, ConcurrentDictionary<string, Guid>, ConcurrentDictionary<Guid, string>> Folders { get; } = new();

    public ConcurrentDictionary<Guid, JsonObject> NodeMetaJson { get; } = new();
    public ConcurrentDictionary<Guid, byte[]> FileData { get; } = new();
    public ConcurrentDictionary<Guid, OsLevelMeta> OsLevelMetaData { get; } = new();

    public Guid CreateFolder(string path)
    {
        var guid = Guid.NewGuid();
        Folders.Forward[path] = guid;
        Folders.Backward[guid] = path;
        return guid;
    }

    public Guid CreateFile(string path)
    {
        var guid = Guid.NewGuid();
        Files.Forward[path] = guid;
        Files.Backward[guid] = path;
        FileData[guid] = Array.Empty<byte>();
        return guid;
    }

    public bool TryGetMetaForPath(string pathValue, out JsonObject? jsonObject)
    {
        if (TryGetGuidForPath(pathValue, out var guid) &&
            NodeMetaJson.TryGetValue(guid, out var metaObj) && metaObj != null)
        {
            jsonObject = metaObj;
            return true;
        }

        jsonObject = null;
        return false;
    }

    private bool TryGetGuidForPath(string pathValue, out Guid guid)
    {
        return Files.Forward.TryGetValue(pathValue, out guid) ||
                    Folders.Forward.TryGetValue(pathValue, out guid);
    }

    public byte[] GetFileData(INodePath path)
    {
        if (Files.Forward.TryGetValue(path.PathValue, out var guid))
        {
            return FileData[guid];
        }
        else
        {
            return Array.Empty<byte>();
        }
    }

    public OsLevelMeta GetOsLevelMeta(INodePath path)
    {
        if (TryGetGuidForPath(path.PathValue, out var guid))
        {
            return OsLevelMetaData.GetOrAdd(guid, guid => OsLevelMeta.Now());
        }
        else
        {
            return OsLevelMeta.Now();
        }
    }

    public Task<Stream> OpenWriteInMemory(INodePath nodePath, CancellationToken cancellationToken)
    {
        return Task.FromResult<Stream>(new MemoryStream());
        // private static Stream OpenWriteInMemory(INodePath path, CancellationToken cancellationToken)
        // {
        //     documentr
        // }
    }

    public Task<IEnumerable<IReadOnlyNode>> Filter(QueryDocumentOptions options, CancellationToken cancellationToken, GenericReadOnlyDocumentResolver genericReadOnlyDocumentResolver, IBroadcastProvider broadcastProvider, Config config)
    {
        var results = options.IncludeFolders ? GetDirectoryAndFileNodes(genericReadOnlyDocumentResolver, broadcastProvider, config) : GetFileNodes(genericReadOnlyDocumentResolver, broadcastProvider, config);
        
        if (!options.IncludeHidden)
        {

        }

        return Task.FromResult(results);
    }

    private IEnumerable<IReadOnlyNode> GetDirectoryAndFileNodes(GenericReadOnlyDocumentResolver resolver, IBroadcastProvider broadcastProvider, Config config)
    {
        return Folders.Forward.Keys.Select(v => new GenericReadOnlyFolder(resolver, broadcastProvider, v.AsIDocPath(config))).ToList().Concat(GetFileNodes(resolver, broadcastProvider, config));
    }

    private IEnumerable<IReadOnlyNode> GetFileNodes(GenericReadOnlyDocumentResolver resolver, IBroadcastProvider broadcastProvider, Config config)
    {
        return Files.Forward.Keys.Select(v => new GenericReadOnlyDocument(resolver, broadcastProvider, v.AsIDocPath(config))).ToList();
    }

    public class OsLevelMeta
    {
        public DateTime CreationTimeUtc { get; set; }
        
        public DateTime LastWriteTimeUtc { get; set; }
        
        public DateTime LastAccessTimeUtc { get; set; }
        
        public DateTime MetaLastWriteTimeUtc { get; set; }

        public static OsLevelMeta Now()
        {
            var now = DateTime.UtcNow;
            return new OsLevelMeta
            {
                CreationTimeUtc = now,
                LastAccessTimeUtc = now,
                LastWriteTimeUtc = now,
                MetaLastWriteTimeUtc = now,
            };
        }
    }
}