using slowpoke.core.Models.Diff;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs.ReadOnlyLocal;


public class ReadOnlyFolder : ReadOnlyNode, IReadOnlyFolder
{
    public override bool Exists => Directory.Exists(this.Path.ConvertToAbsolutePath().PathValue);

    public override long SizeBytes => Permissions.CanRead ? OnUnauthorizedReturn0(() => Directory.EnumerateFiles(this.Path.ConvertToAbsolutePath().PathValue).Sum(f => OnUnauthorizedReturn0(() => new FileInfo(f).Length))) : 0L;

    public int SizeFiles => Permissions.CanRead ? OnUnauthorizedReturn0(() => Directory.EnumerateFiles(this.Path.ConvertToAbsolutePath().PathValue).Count()) : 0;

    public int SizeFolders => Permissions.CanRead ? OnUnauthorizedReturn0(() => Directory.EnumerateDirectories(this.Path.ConvertToAbsolutePath().PathValue).Count()) : 0;

    public override NodePermissionCategories<bool> Permissions
    {
        get
        {
            return new NodePermissionCategories<bool>
            {
                CanRead = Exists && new DirectoryInfo(this.Path.ConvertToAbsolutePath().PathValue).CreationTime != null,
                CanWrite = Exists && new DirectoryInfo(this.Path.ConvertToAbsolutePath().PathValue).CreationTime != null,
                IsEncrypted = false,
                LimitedToUserOnly = false,
                LimitedToMachineOnly = true,
                LimitedToAllowedConnectionsOnly = false,
                LimitedToLocalNetworkOnly = false,
                UnlimitedUniversalPublicAccess = false,
            };
        }
    }

    public ReadOnlyFolder(IReadOnlyDocumentResolver documentResolver, IBroadcastProvider broadcastProvider, INodePath path) : base(documentResolver, broadcastProvider, path)
    {
        if (!this.Path.IsFolder)
            throw new ArgumentException("path is not a folder", nameof(this.Path));
    }

    public int CompareTo(IReadOnlyFolder? other)
    {
        throw new NotImplementedException();
    }

    public bool Equals(IReadOnlyFolder? other) => CompareTo(other) == 0;

    public override void Sync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override void BroadcastChanges(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override void PollForChanges(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override void TurnOnSync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override void TurnOffSync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override string ComputeHash()
    {
        throw new NotImplementedException();
    }

    public override void MergeChanges(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<INodeDiffBrief> FetchChanges(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override INodeFingerprint GetFingerprint(CancellationToken cancellationToken)
    {
        return new NodeFingerprintModel(Path, ComputeHash(), Meta.LastUpdate, Meta.ComputeMetaHash(), Meta.LastMetaUpdate);
    }
}