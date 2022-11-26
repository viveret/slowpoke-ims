using System.Data;
using slowpoke.core.Models.Diff;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs.ReadOnlyLocal;


public class GenericReadOnlyFolder : ReadOnlyNode, IReadOnlyFolder
{
    public override Task<bool> Exists => documentResolver.NodeExistsAtPath(this.Path, CancellationToken.None);

    public override Task<long> SizeBytes => GetSizeBytesAsync();

    private async Task<long> GetSizeBytesAsync()
    {
        return (await Permissions).CanRead ? OnUnauthorizedReturn0(() => Directory.EnumerateFiles(this.Path.ConvertToAbsolutePath().PathValue).Sum(f => OnUnauthorizedReturn0(() => new FileInfo(f).Length))) : 0L;
    }

    public int SizeFiles => Permissions.Result.CanRead ? OnUnauthorizedReturn0(() => Directory.EnumerateFiles(this.Path.ConvertToAbsolutePath().PathValue).Count()) : 0;

    public int SizeFolders => Permissions.Result.CanRead ? OnUnauthorizedReturn0(() => Directory.EnumerateDirectories(this.Path.ConvertToAbsolutePath().PathValue).Count()) : 0;

    public override Task<NodePermissionCategories<bool>> Permissions
    {
        get
        {
            return Task.FromResult(new NodePermissionCategories<bool>
            {
                CanRead = true,
                CanWrite = true,
                IsEncrypted = false,
                LimitedToUserOnly = false,
                LimitedToMachineOnly = true,
                LimitedToAllowedConnectionsOnly = false,
                LimitedToLocalNetworkOnly = false,
                UnlimitedUniversalPublicAccess = false,
            });
        }
    }

    public GenericReadOnlyFolder(IReadOnlyDocumentResolver documentResolver, IBroadcastProvider broadcastProvider, INodePath path) : base(documentResolver, broadcastProvider, path)
    {
        if (!this.Path.IsFolder)
            throw new ArgumentException("path is not a folder", nameof(this.Path));
    }

    public int CompareTo(IReadOnlyFolder? other)
    {
        throw new NotImplementedException();
    }

    public bool Equals(IReadOnlyFolder? other) => CompareTo(other) == 0;

    public override Task Sync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override Task BroadcastChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override Task PollForChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override Task TurnOnSync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override Task TurnOffSync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override Task<string> ComputeHash()
    {
        // todo: this needs to be re-implemented
        return Task.FromResult(new System.IO.DirectoryInfo(Path.PathValue).LastWriteTimeUtc.ToString());
    }

    public override Task MergeChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
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
}