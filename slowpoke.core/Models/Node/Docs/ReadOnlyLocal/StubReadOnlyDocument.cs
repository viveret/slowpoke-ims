using System.Data;
using System.Security.Cryptography;
using System.Text;
using slowpoke.core.Extensions;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Diff;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs.ReadOnlyLocal;


public class StubReadOnlyDocument : StubReadOnlyNode, IReadOnlyDocument
{
    private readonly bool exists;

    public StubReadOnlyDocument(bool exists, INodePath path, Config config): base(config, path)
    {
        this.exists = exists;
    }

    public Task<Stream> OpenRead() => Task.FromResult(System.IO.Stream.Null);

    // different types of equals, this one performs hash equality (not byte equality)
    public async Task<bool> Equals(IReadOnlyDocument? other) => await CompareTo(other) == 0;

    public async Task<int> CompareTo(IReadOnlyDocument? other)
    {
        if (other != null)
        {
            using var otherStream = await other.OpenRead();
            using var stream = await OpenRead();
            var otherStreamMD5 = otherStream.ComputeMD5FromStream(false);
            var thisStreamMD5 = stream.ComputeMD5FromStream(false);

            return thisStreamMD5.CompareTo(otherStreamMD5);
        }
        return -1;
    }

    public async Task<string> ReadAllText(Encoding encoding, int numLines = 0) => string.Empty;

    public override Task Sync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override Task BroadcastChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        //BroadcastProvider.Publish(new ChangedMessage());
    }

    public override Task PollForChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        //BroadcastProvider.Receive();
    }

    public override Task TurnOnSync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override Task TurnOffSync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        // Meta.SyncEnabled = false;
    }

    public override async Task<string> ComputeHash()
    {
        using var stream = await OpenRead();
        return stream.ComputeMD5FromStream(false);
    }

    public override Task MergeChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        //documentResolver.MergeChangesForNode(this, cancellationToken);
    }

    public override Task<IEnumerable<INodeDiffBrief>> FetchChanges(CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Empty<INodeDiffBrief>());
    }

    public override async Task<INodeFingerprint> GetFingerprint(CancellationToken cancellationToken)
    {
        var meta = await Meta;
        return new NodeFingerprintModel(Path, await ComputeHash(), meta.LastUpdate, meta.ComputeMetaHash(), meta.LastMetaUpdate);
    }

    public override Task<bool> Exists => Task.FromResult(exists);

    public override Task<long> SizeBytes => Task.FromResult(0L);

    public Task<string> GetContentType() => Task.FromResult(string.Empty);

    public override Task<NodePermissionCategories<bool>> Permissions
    {
        get
        {
            return Task.FromResult(new NodePermissionCategories<bool>
            {
                CanRead = true,
                CanWrite = false,
                LimitedToUserOnly = false,
                LimitedToMachineOnly = true,
                LimitedToAllowedConnectionsOnly = false,
                LimitedToLocalNetworkOnly = false,
                UnlimitedUniversalPublicAccess = false,
            });
        }
    }
}