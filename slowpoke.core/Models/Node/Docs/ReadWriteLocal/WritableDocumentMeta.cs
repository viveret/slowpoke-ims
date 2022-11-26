using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs.ReadWriteLocal;


public class WritableDocumentMeta : ReadOnlyDocumentMeta, IWritableDocumentMeta
{
    private readonly IWritableDocumentResolver writableDocResolver;
    private MemoryStream? tmpStream;

    public WritableDocumentMeta(IWritableDocumentResolver docResolver, INodePath path) : base(docResolver, path)
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
            FileInfo.CreationTimeUtc = value;
            MetaJson[nameof(CreationDate)] = value;
        }
    }

    DateTime? IWritableDocumentMeta.LastUpdate
    {
        set
        {
            FileInfo.LastWriteTimeUtc = value ?? DateTime.UtcNow;
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
            await metaStreamDestination.WriteAsync(System.Text.Encoding.UTF8.GetBytes(str));
        }



        // var old = new ReadOnlyDocument(DocumentResolver, Path);
        // var streamDiff = new StreamDiff(writer, old.OpenRead(), cancellationToken);
        
        // var diff = old != null ? new DocumentDiff(old, this) : null;

        // if (diff == null || diff.HasChanged)
        // {
        //     // save to local file
        //     var path = this.Path.ConvertToAbsolutePath().PathValue;
        //     var parentPath = System.IO.Path.GetDirectoryName(path);
        //     if (parentPath != null && !Directory.Exists(parentPath))
        //     {
        //         Directory.CreateDirectory(parentPath);
        //     }

        //     // file is being used by another process - this is broken
        //     using (var streamDestination = File.OpenWrite(path))
        //     {
        //         writer(streamDestination);
        //     }
            
        //     // save meta to local file            
        //     WriteMeta(cancellationToken);
        // }
    }
}