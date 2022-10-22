using slowpoke.core.Client;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Services.Node.Docs;


public class HttpReadWriteRemoteDocumentResolver : HttpReadOnlyRemoteDocumentResolver, IHttpReadWriteRemoteDocumentResolver
{
    public HttpReadWriteRemoteDocumentResolver(ISlowPokeClient client, IDocumentProviderResolver documentProviderResolver, Config config) : base(client, documentProviderResolver, config)
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