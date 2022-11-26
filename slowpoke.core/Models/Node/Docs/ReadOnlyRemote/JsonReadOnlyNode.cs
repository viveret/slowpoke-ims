using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using slowpoke.core.Models.Diff;

namespace slowpoke.core.Models.Node.Docs.ReadOnlyRemote;


public class JsonReadOnlyNode: IReadOnlyNode
{
    public JsonReadOnlyNode() { }
    
    protected JsonReadOnlyNode(IReadOnlyNode other)
    {
        Path = other.Path;
        SlowPokeId = other.SlowPokeId;
    }

    public static async Task<JsonReadOnlyNode> CreateNode(IReadOnlyNode other)
    {
        var ret = new JsonReadOnlyNode(other);
        await ret.InitAsync(other);
        return ret;
    }

    protected virtual async Task InitAsync(IReadOnlyNode other)
    {
        MetaSynch = await ReadOnlyDocumentMetaModel.Create(await other.Meta);
        HostSynch = await other.Host;
        PermissionsSynch = await other.Permissions;
        ExistsSynch = await other.Exists;
        HasMetaSynch = await other.HasMeta;
        SizeBytesSynch = await other.SizeBytes;
        CanSyncSynch = await other.CanSync;
    }

    public INodePath Path { get; set; } = new NodePathModel();

    public ISlowPokeId SlowPokeId { get; set; } = new SlowPokeIdModel();

    [JsonIgnore, IgnoreDataMember]
    public Task<IReadOnlyDocumentMeta> Meta => Task.FromResult<IReadOnlyDocumentMeta>(MetaSynch);
    
    //[JsonPropertyName(nameof(Meta))]
    public ReadOnlyDocumentMetaModel MetaSynch { get; set; } = new ReadOnlyDocumentMetaModel();

    [JsonIgnore, IgnoreDataMember]
    public Task<ISlowPokeHost> Host => Task.FromResult(HostSynch);
    
    //[JsonPropertyName(nameof(Host))]
    public ISlowPokeHost HostSynch { get; set; } = new SlowPokeHostModel();

    [JsonIgnore, IgnoreDataMember]
    public Task<NodePermissionCategories<bool>> Permissions => Task.FromResult(PermissionsSynch);

    //[JsonPropertyName(nameof())]    
    public NodePermissionCategories<bool> PermissionsSynch { get; set; } = new NodePermissionCategories<bool>();

    [JsonIgnore, IgnoreDataMember]
    public Task<bool> Exists => Task.FromResult(ExistsSynch);
    
    public bool ExistsSynch { get; set; }

    [JsonIgnore, IgnoreDataMember]
    public Task<bool> HasMeta => Task.FromResult(HasMetaSynch);
    
    public bool HasMetaSynch { get; set; }

    [JsonIgnore, IgnoreDataMember]
    public Task<long> SizeBytes => Task.FromResult(SizeBytesSynch);
    
    public long SizeBytesSynch { get; set; }

    [JsonIgnore, IgnoreDataMember]
    public Task<bool> CanSync => Task.FromResult(CanSyncSynch);
    
    public bool CanSyncSynch { get; set; }

    public Task BroadcastChanges(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> CompareTo(IReadOnlyNode other)
    {
        throw new NotImplementedException();
    }

    public Task<string> ComputeHash()
    {
        throw new NotImplementedException();
    }

    public Task<bool> Equals(IReadOnlyNode other)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<INodeDiffBrief>> FetchChanges(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<INodeFingerprint> GetFingerprint(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task MergeChanges(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Sync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task TurnOffSync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task TurnOnSync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}