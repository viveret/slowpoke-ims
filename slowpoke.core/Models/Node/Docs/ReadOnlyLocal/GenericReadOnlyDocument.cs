using System.Data;
using System.Security.Cryptography;
using System.Text;
using slowpoke.core.Extensions;
using slowpoke.core.Models.Diff;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;

namespace slowpoke.core.Models.Node.Docs.ReadOnlyLocal;


public class GenericReadOnlyDocument : ReadOnlyNode, IReadOnlyDocument
{
    public Stream? TmpStream { get; set; } // temporary

    public GenericReadOnlyDocument(
        IReadOnlyDocumentResolver documentResolver,
        IBroadcastProvider broadcastProvider,
        INodePath path): base(documentResolver, broadcastProvider, path)
    {
        if (!this.Path.IsDocument)
            throw new ArgumentException($"path is not a document: {path}", nameof(this.Path));
    }

    public async Task<Stream> OpenRead() => TmpStream ?? (await Exists ? new MemoryStream(((GenericReadOnlyDocumentResolver)documentResolver).inMemoryGenericDocumentRepository.GetFileData(Path)) : System.IO.Stream.Null);

    // different types of equals, this one performs hash equality (not byte equality)
    public async Task<bool> Equals(IReadOnlyDocument? other) => await CompareTo(other) == 0;

    public Task<int> CompareTo(IReadOnlyDocument? other)
    {
        throw new NotImplementedException();
        // if (other != null)
        // {
        //     using var otherStream = other.OpenRead();
        //     using var stream = OpenRead();
        //     var otherStreamMD5 = otherStream.ComputeMD5FromStream();
        //     var thisStreamMD5 = stream.ComputeMD5FromStream();

        //     return thisStreamMD5.CompareTo(otherStreamMD5);
        // }
        // return -1;
    }

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

    public Task<string> ReadAllText(Encoding encoding, int numLines = 0)
    {
        return Task.FromResult("");
    }

    public override Task<IEnumerable<INodeDiffBrief>> FetchChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override async Task<INodeFingerprint> GetFingerprint(CancellationToken cancellationToken)
    {
        var meta = await Meta;
        return new NodeFingerprintModel(Path, await ComputeHash(), meta.LastUpdate, meta.ComputeMetaHash(), meta.LastMetaUpdate);
    }

    // public override IEnumerable<INodeDiffBrief> FetchChanges(CancellationToken cancellationToken)
    // {
    //     var currentVersion = GetFingerprint(cancellationToken);
    //     var otherVersions = documentResolver.FetchFingerprintsForNode(this, cancellationToken);
    //     return otherVersions.Select(v => new NodeDiffBrief(currentVersion, v));
    // }

    // public override INodeFingerprint GetFingerprint(CancellationToken cancellationToken)
    // {
    //     return new NodeFingerprintModel(Path, ComputeHash(), Meta.LastUpdate, Meta.ComputeMetaHash(), Meta.LastMetaUpdate);
    // }

    public override Task<bool> Exists => documentResolver.NodeExistsAtPath(this.Path, CancellationToken.None);

    public override Task<long> SizeBytes => GetSizeBytesAsync();
    
    private async Task<long> GetSizeBytesAsync()
    {
        return await Exists ? OnUnauthorizedReturn0(() => -1) : 0L;
    }

    public Task<string> GetContentType() => documentResolver.GetContentTypeFromExtension(this.Path.PathValue.GetFullExtension());

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