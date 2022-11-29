using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadOnlyLocal;

namespace slowpoke.core.Services.Node.Docs;

public interface IReadOnlyDocumentResolver
{
    string InstanceName { get; }
    
    string ResolverTypeName { get; }

    Task<ISlowPokeHost> Host { get; }

    Task<bool> CanSync { get; }
    
    Task<NodePermissionCategories<bool>> Permissions { get; }

    Task<IReadOnlyNode> GetNodeAtPath(INodePath path, CancellationToken cancellationToken);

    Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken);

    Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken);

    Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken);
    
    Task<int> GetCountOfNodes(CancellationToken cancellationToken);
    
    Task<int> GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken);
    
    Task<IEnumerable<IReadOnlyNode>> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken);

    Task<int> GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken);
    
    Task<IEnumerable<IReadOnlyNode>> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken);

    Task<string> GetExtensionFromContentType(string contentType);
    
    Task<string> GetContentTypeFromExtension(string extension);
    
    Task<IEnumerable<INodeFingerprint>> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken);
}
