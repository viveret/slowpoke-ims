using System.Data;
using System.Security.Cryptography;
using System.Text;
using slowpoke.core.Extensions;
using slowpoke.core.Models.Diff;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs.ReadOnlyRemote;


public class RemoteReadOnlyNode: IReadOnlyNode
{
    private IReadOnlyDocumentMeta meta;
    private ISlowPokeHost host;
    private NodePermissionCategories<bool> permissions;
    private bool exists;
    private bool hasMeta;
    private long sizeBytes;
    private bool canSync;

    protected RemoteReadOnlyNode(IReadOnlyNode modelNode, IReadOnlyDocumentResolver documentResolver)
    {
        ArgumentNullException.ThrowIfNull(modelNode);
        Path = modelNode.Path ?? throw new ArgumentNullException(nameof(modelNode.Path));
        SlowPokeId = modelNode.SlowPokeId ?? throw new ArgumentNullException(nameof(modelNode.SlowPokeId));
    }

    public static async Task<RemoteReadOnlyNode> CreateNode(IReadOnlyNode modelNode, IReadOnlyDocumentResolver documentResolver)
    {
        var ret = new RemoteReadOnlyNode(modelNode, documentResolver);
        await ret.InitAsync(modelNode);
        return ret;
    }

    protected virtual async Task InitAsync(IReadOnlyNode modelNode)
    {
        meta = await modelNode.Meta;
        host = await modelNode.Host;
        permissions = await modelNode.Permissions;
        exists = await modelNode.Exists;
        hasMeta = await modelNode.HasMeta;
        sizeBytes = await modelNode.SizeBytes;
        canSync = await modelNode.CanSync;
    }

    public INodePath Path { get; }

    public ISlowPokeId SlowPokeId { get; }

    public Task<IReadOnlyDocumentMeta> Meta => Task.FromResult(meta);

    public Task<ISlowPokeHost> Host => Task.FromResult(host);

    public Task<NodePermissionCategories<bool>> Permissions => Task.FromResult(permissions);

    public Task<bool> Exists => Task.FromResult(exists);

    public Task<bool> HasMeta => Task.FromResult(hasMeta);

    public Task<long> SizeBytes => Task.FromResult(sizeBytes);

    public Task<bool> CanSync => Task.FromResult(canSync);

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