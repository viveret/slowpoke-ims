using System.IO;
using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Services.Node.Docs.ReadOnlyLocal;


public class ReadOnlyLocalDriveDocumentProvider : IReadOnlyDocumentResolver
{
    public DriveInfo? DriveInfo { get; }
    public IDocumentProviderResolver DocumentProviderResolver { get; }

    public ReadOnlyLocalDriveDocumentProvider(DriveInfo d, IDocumentProviderResolver documentProviderResolver)
    {
        DriveInfo = d;
        DocumentProviderResolver = documentProviderResolver;
    }

    public string InstanceName => $"{ResolverTypeName} for {HostName}";

    public string HostName => $"localhost/${DriveName}";

    public string DriveName => DriveInfo?.Name ?? throw new Exception("No device name available");

    public string ResolverTypeName => GetType().Name;

    public Task<ISlowPokeHost> Host => DocumentProviderResolver.Host;

    public Task<NodePermissionCategories<bool>> Permissions
    {
        get
        {
            return Task.FromResult(new NodePermissionCategories<bool> { });
        }
    }

    public Task<bool> CanSync => Task.FromResult(false);

    public Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetContentTypeFromExtension(string extension)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountOfNodes(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountOfDocuments(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyNode> GetNodeAtPath(INodePath path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IReadOnlyDocument>> GetDocuments(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IReadOnlyDocument>> GetDocumentsInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetExtensionFromContentType(string contentType)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IReadOnlyNode>> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IReadOnlyNode>> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<INodePath>> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<INodeFingerprint>> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}