using slowpoke.core.Client;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Services.Node.Docs;


public class HttpReadWriteRemoteDocumentResolver : HttpReadOnlyRemoteDocumentResolver, IHttpReadWriteRemoteDocumentResolver
{
    public HttpReadWriteRemoteDocumentResolver(ISlowPokeClient client, IDocumentProviderResolver documentProviderResolver, Config config) : base(client, config, documentProviderResolver)
    {
    }

    public Task ArchiveDocumentAtPath(INodePath path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task ArchiveDocumentsInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IWritableDocument> CopyDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteDocumentAtPath(INodePath path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteDocumentsInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IWritableDocument> MoveDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IWritableDocument> NewDocument(NewDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> OpenWriteMeta(IWritableDocumentMeta docMeta, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}