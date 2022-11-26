using slowpoke.core.Models.Diff;
using SlowPokeIMS.Core.Collections;

namespace slowpoke.core.Models.Node.Docs;

public interface IReadOnlyNode: IAsyncEquatable<IReadOnlyNode>, IAsyncComparable<IReadOnlyNode>
{
    INodePath Path { get; }

    ISlowPokeId SlowPokeId { get; }

    Task<IReadOnlyDocumentMeta> Meta { get; }

    Task<ISlowPokeHost> Host { get; }

    Task<NodePermissionCategories<bool>> Permissions { get; }

    Task<bool> Exists { get; }
    
    Task<bool> HasMeta { get; }

    Task<long> SizeBytes { get; }

    Task<string> ComputeHash();

    // if the node is enabled and the host has enabled syncing
    Task<bool> CanSync { get; }

    // Does a poll for changes and checks if current file will collide with incoming change
    // Also broadcasts out changes if there are any
    Task Sync(CancellationToken cancellationToken);

    Task BroadcastChanges(CancellationToken cancellationToken);
    
    Task MergeChanges(CancellationToken cancellationToken);
    
    Task<IEnumerable<INodeDiffBrief>> FetchChanges(CancellationToken cancellationToken);
    
    Task TurnOnSync(CancellationToken cancellationToken);
    
    Task TurnOffSync(CancellationToken cancellationToken);
    
    Task<INodeFingerprint> GetFingerprint(CancellationToken cancellationToken);
}
