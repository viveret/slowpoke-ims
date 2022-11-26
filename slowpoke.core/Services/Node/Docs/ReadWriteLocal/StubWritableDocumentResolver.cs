using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;

namespace SlowPokeIMS.Core.Services.Node.Docs.ReadWrite;



public class StubWritableDocumentResolver : StubReadOnlyDocumentResolver, IWritableDocumentResolver
{
    public StubWritableDocumentResolver(Config config) : base(config)
    {
    }

    public Task ArchiveDocumentAtPath(INodePath path, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task ArchiveDocumentsInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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