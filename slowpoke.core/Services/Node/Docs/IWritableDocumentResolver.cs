using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Services.Node.Docs;

// should this manage folders as well?
public interface IWritableDocumentResolver: IReadOnlyDocumentResolver
{
    Stream OpenWriteMeta(IWritableDocumentMeta docMeta, CancellationToken cancellationToken);
    
    IWritableDocument NewDocument(NewDocumentOptions options, CancellationToken cancellationToken);
    
    IWritableDocument CopyDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken);
    
    IWritableDocument MoveDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken);
    
    void ArchiveDocumentAtPath(INodePath path, CancellationToken cancellationToken);
    
    void DeleteDocumentAtPath(INodePath path, CancellationToken cancellationToken);
    
    void ArchiveDocumentsInFolder(INodePath folder, CancellationToken cancellationToken);
    
    void DeleteDocumentsInFolder(INodePath folder, CancellationToken cancellationToken);
}
