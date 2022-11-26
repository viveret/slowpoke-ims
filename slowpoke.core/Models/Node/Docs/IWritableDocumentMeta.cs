namespace slowpoke.core.Models.Node.Docs;


public interface IWritableDocumentMeta : IReadOnlyDocumentMeta
{
    new string Title { set; }
    new bool SyncEnabled { set; }
    new bool Favorited { set; }
    new string ContentType { set; }
    new DateTime CreationDate { set; }
    new DateTime? LastUpdate { set; }
    new DateTime? LastMetaUpdate { set; }
    new DateTime? ArchivedDate { set; }
    new DateTime? DeletedDate { set; }

    Task<Stream> OpenWriteMetaOnDisk(CancellationToken cancellationToken);

    Task WriteIfChanged(bool touch = true, CancellationToken cancellationToken = default);
}