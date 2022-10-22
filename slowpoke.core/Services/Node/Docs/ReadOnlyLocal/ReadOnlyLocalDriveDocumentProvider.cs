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

    public ISlowPokeHost Host => DocumentProviderResolver.Host;

    public NodePermissionCategories<bool> Permissions
    {
        get
        {
            return new NodePermissionCategories<bool> { };
        }
    }

    public bool CanSync => false;

    public bool NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public string GetContentTypeFromExtension(string extension)
    {
        throw new NotImplementedException();
    }

    public int GetCountOfNodes(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public int GetCountOfDocuments(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public int GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public int GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyDocument GetNodeAtPath(INodePath path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IReadOnlyDocument> GetDocuments(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IReadOnlyDocument> GetDocumentsInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public string GetExtensionFromContentType(string contentType)
    {
        throw new NotImplementedException();
    }

    public bool HasMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    IReadOnlyNode IReadOnlyDocumentResolver.GetNodeAtPath(INodePath path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyDocumentMeta GetMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IReadOnlyNode> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IReadOnlyNode> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<INodePath> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public int GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<INodeFingerprint> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}