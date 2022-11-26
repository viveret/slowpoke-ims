using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs.ReadWriteLocal;


public class GenericWritableDocumentMeta : GenericReadOnlyDocumentMeta, IWritableDocumentMeta
{
    private readonly IWritableDocumentResolver writableDocResolver;

    public GenericWritableDocumentMeta(GenericReadWriteDocumentResolver docResolver, INodePath path) : base(docResolver, path)
    {
        writableDocResolver = docResolver;
    }

    string IWritableDocumentMeta.Title
    {
        set
        {
            MetaJson[nameof(Title)] = value ?? string.Empty;
        }
    }

    string IWritableDocumentMeta.ContentType
    {
        set
        {
            MetaJson[nameof(ContentType)] = value;
        }
    }

    DateTime IWritableDocumentMeta.CreationDate
    {
        set
        {
            MetaJson[nameof(CreationDate)] = value;
        }
    }

    DateTime? IWritableDocumentMeta.LastUpdate
    {
        set
        {
            MetaJson[nameof(LastUpdate)] = value ?? DateTime.UtcNow;
        }
    }

    DateTime? IWritableDocumentMeta.LastMetaUpdate
    {
        set
        {
            MetaJson[nameof(LastMetaUpdate)] = value ?? DateTime.UtcNow;
        }
    }

    DateTime? IWritableDocumentMeta.ArchivedDate
    {
        set
        {
            MetaJson[nameof(ArchivedDate)] = value ?? DateTime.UtcNow;
        }
    }
    
    DateTime? IWritableDocumentMeta.DeletedDate
    {
        set
        {
            MetaJson[nameof(DeletedDate)] = value ?? DateTime.UtcNow;
        }
    }

    bool IWritableDocumentMeta.SyncEnabled
    {
        set
        {
            MetaJson[nameof(SyncEnabled)] = value;
        }
    }

    bool IWritableDocumentMeta.Favorited
    {
        set
        {
            MetaJson[nameof(Favorited)] = value;
        }
    }

    public Task<Stream> OpenWriteMetaOnDisk(CancellationToken cancellationToken) => writableDocResolver.OpenWriteMeta(this, cancellationToken);

    public async Task WriteIfChanged(bool touch = false, CancellationToken cancellationToken = default)
    {
        if (touch)
        {
            (this as IWritableDocumentMeta).LastMetaUpdate = DateTime.UtcNow;
        }

        using (var metaStreamDestination = await OpenWriteMetaOnDisk(cancellationToken))
        {
            var str = MetaJson.ToString();
            metaStreamDestination.Write(System.Text.Encoding.UTF8.GetBytes(str));
        }
    }
}