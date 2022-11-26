using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Services.Node.Docs;

// should this manage folders as well?
public interface IWritableDocumentResolver: IReadOnlyDocumentResolver
{
    Task<Stream> OpenWriteMeta(IWritableDocumentMeta docMeta, CancellationToken cancellationToken);
    
    Task<IWritableDocument> NewDocument(NewDocumentOptions options, CancellationToken cancellationToken);
    
    Task<IWritableDocument> CopyDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken);
    
    Task<IWritableDocument> MoveDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken);
    
    Task ArchiveDocumentAtPath(INodePath path, CancellationToken cancellationToken);
    
    Task DeleteDocumentAtPath(INodePath path, CancellationToken cancellationToken);
    
    Task ArchiveDocumentsInFolder(INodePath folder, CancellationToken cancellationToken);
    
    Task DeleteDocumentsInFolder(INodePath folder, CancellationToken cancellationToken);
}
