namespace slowpoke.core.Models.Node.Docs;

public interface IWritableFolder: IReadOnlyFolder
{
    Task<IWritableDocumentMeta> GetWritableMeta(CancellationToken cancellationToken);

    Task WriteMeta(CancellationToken cancellationToken);
}