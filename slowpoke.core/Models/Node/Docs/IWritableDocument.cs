namespace slowpoke.core.Models.Node.Docs;

public interface IWritableDocument: IReadOnlyDocument
{
    void WriteIfChanged(Action<Stream> writer, CancellationToken cancellationToken);

    IWritableDocumentMeta GetWritableMeta(CancellationToken cancellationToken);

    void WriteMeta(CancellationToken cancellationToken);
}