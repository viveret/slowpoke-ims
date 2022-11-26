namespace slowpoke.core.Models.Node.Docs;

public interface IWritableDocument: IReadOnlyDocument
{
    Task WriteIfChanged(Action<Stream> writer, CancellationToken cancellationToken);

    Task<IWritableDocumentMeta> GetWritableMeta(CancellationToken cancellationToken);

    Task WriteMeta(CancellationToken cancellationToken);
}