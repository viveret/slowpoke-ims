using slowpoke.core.Models.Diff;

namespace slowpoke.core.Models.Node.Docs;

public interface IReadOnlyNode: IEquatable<IReadOnlyNode>, IComparable<IReadOnlyNode>
{
    INodePath Path { get; }

    ISlowPokeId SlowPokeId { get; }

    IReadOnlyDocumentMeta Meta { get; }

    ISlowPokeHost Host { get; }

    NodePermissionCategories<bool> Permissions { get; }

    bool Exists { get; }
    
    bool HasMeta { get; }

    long SizeBytes { get; }

    string ComputeHash();

    // if the node is enabled and the host has enabled syncing
    bool CanSync { get; }

    // Does a poll for changes and checks if current file will collide with incoming change
    // Also broadcasts out changes if there are any
    void Sync(CancellationToken cancellationToken);

    void BroadcastChanges(CancellationToken cancellationToken);
    
    void MergeChanges(CancellationToken cancellationToken);
    
    IEnumerable<INodeDiffBrief> FetchChanges(CancellationToken cancellationToken);
    
    void TurnOnSync(CancellationToken cancellationToken);
    
    void TurnOffSync(CancellationToken cancellationToken);
    
    INodeFingerprint GetFingerprint(CancellationToken cancellationToken);
}
