using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadOnlyLocal;

namespace slowpoke.core.Services.Node.Docs;

public interface IReadOnlyDocumentResolver
{
    string InstanceName { get; }
    
    string ResolverTypeName { get; }

    ISlowPokeHost Host { get; }

    bool CanSync { get; }
    
    NodePermissionCategories<bool> Permissions { get; }

    IReadOnlyNode GetNodeAtPath(INodePath path, CancellationToken cancellationToken);

    IReadOnlyDocumentMeta GetMeta(IReadOnlyNode node, CancellationToken cancellationToken);
    
    bool HasMeta(IReadOnlyNode node, CancellationToken cancellationToken);

    bool NodeExistsAtPath(INodePath path, CancellationToken cancellationToken);
    
    int GetCountOfNodes(CancellationToken cancellationToken);
    
    int GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken);
    
    IEnumerable<IReadOnlyNode> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken);

    int GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken);
    
    IEnumerable<IReadOnlyNode> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken);

    IEnumerable<INodePath> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken);
    
    int GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken);

    string GetExtensionFromContentType(string contentType);
    
    string GetContentTypeFromExtension(string extension);
    
    IEnumerable<INodeFingerprint> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken);
}
