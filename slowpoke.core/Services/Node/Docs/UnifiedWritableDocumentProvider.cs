using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Services.Node.Docs;


public class UnifiedWritableDocumentProvider : UnifiedReadOnlyDocumentProvider, IWritableDocumentResolver
{
    public UnifiedWritableDocumentProvider(
        LocalDocumentProviderResolver documentProviderResolver,
        IEnumerable<IWritableDocumentResolver> documentResolvers) : base(documentProviderResolver, documentResolvers)
    {
    }

    public void ArchiveDocumentAtPath(INodePath path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void ArchiveDocumentsInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IWritableDocument CopyDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void DeleteDocumentAtPath(INodePath path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void DeleteDocumentsInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IWritableDocument MoveDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IWritableDocument NewDocument(NewDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Stream OpenWriteMeta(IWritableDocumentMeta docMeta, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}