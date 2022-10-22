using System.Data;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadWriteLocal;
using slowpoke.core.Services.Broadcast;

namespace slowpoke.core.Services.Node.Docs;


public class WritableLocalDocumentResolver : ReadOnlyLocalDocumentResolver, IWritableDocumentResolver
{
    public override NodePermissionCategories<bool> Permissions
    {
        get
        {
            var perms = base.Permissions;
            perms.CanWrite = true;
            return perms;
        }
    }

    public WritableLocalDocumentResolver(Config config, IBroadcastProviderResolver broadcastProviderResolver) : base(config, broadcastProviderResolver)
    {
    }

    public override IReadOnlyNode GetNodeAtPath(INodePath path, CancellationToken cancellationToken)
    {
        var docReadonly = base.GetNodeAtPath(path, cancellationToken);
        if (docReadonly.Exists && Permissions.CanWrite)
        {
            return new WritableDocument(this, broadcastProviderResolver.MemCached, path);
        }
        return docReadonly;
    }

    public override IReadOnlyDocumentMeta GetMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        var metaReadonly = base.GetMeta(node, cancellationToken);
        if (Permissions.CanWrite)
        {
            return new WritableDocumentMeta(this, metaReadonly.Path);
        }
        return metaReadonly;
    }

    public void ArchiveDocumentAtPath(INodePath path, CancellationToken cancellationToken)
    {
        var doc = GetNodeAtPath(path, cancellationToken);
        ArchiveNode(doc, cancellationToken);
    }

    private void ArchiveNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        if (node is IReadOnlyDocument doc)
        {
            ArchiveDocument(doc, cancellationToken);
        }
        else if (node is IReadOnlyFolder folder)
        {
            ArchiveDocumentsInFolder(folder.Path, cancellationToken);
        }
        else
        {
            throw new NotSupportedException($"Node type {node.GetType().Name} not supported");
        }
    }

    private void ArchiveDocument(IReadOnlyDocument doc, CancellationToken cancellationToken)
    {
        GetMetaOrThrowIfNotWritable(doc, writable =>
        {
            writable.ArchivedDate = DateTime.UtcNow;
            writable.WriteIfChanged(cancellationToken: cancellationToken);
        });
    }

    // todo: fix this, if client calls with using() then the stream will be closed prematurely
    public Stream OpenWrite(WritableDocument doc, CancellationToken cancellationToken) => OpenWriteLocal(doc.Path, cancellationToken);

    public Stream OpenWriteMeta(IWritableDocumentMeta doc, CancellationToken cancellationToken) => OpenWriteLocal(doc.Path.ConvertToAbsolutePath().ConvertToMetaPath(), cancellationToken);

    private static Stream OpenWriteLocal(INodePath path, CancellationToken cancellationToken)
    {
        var p = path.ConvertToAbsolutePath().PathValue;
        if (string.IsNullOrWhiteSpace(p))
            throw new ArgumentNullException(nameof(path));
        else if (File.Exists(p))
            return File.Open(p, FileMode.Create);
        else
        {
            var parentDirectory = Directory.GetParent(p);
            if (parentDirectory == null)
            {
                var parentDirectoryPath = Path.GetDirectoryName(p);
                Directory.CreateDirectory(parentDirectoryPath!);
            }
            else if (!parentDirectory.Exists)
            {
                parentDirectory.Create();
            }
            return File.Create(p);
        }
    }

    private void GetMetaOrThrowIfNotWritable(IReadOnlyDocument doc, Action<IWritableDocumentMeta> action)
    {
        var meta = doc.Meta;
        if (meta is IWritableDocumentMeta writable)
        {
            action(writable);
        }
        else
        {
            throw new ReadOnlyException();
        }
    }

    public void ArchiveDocumentsInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        ForEachNodeInFolder(folder, (doc) =>
        {
            ArchiveDocument((IReadOnlyDocument)doc, cancellationToken);
        }, cancellationToken);
    }

    private void ForEachPaged<T>(int count, int pageSize, Func<int, IEnumerable<T>> getter, Action<T> foreachFn, CancellationToken cancellationToken)
    {
        for (int i = 0; i < count; i += pageSize)
        {
            foreach (var doc in getter(i))
            {
                foreachFn(doc);
            }
        }
    }

    private void ForEachNodeInFolder(INodePath folder, Action<IReadOnlyNode> foreachFn, CancellationToken cancellationToken)
    {
        var count = GetCountOfNodesInFolder(folder, cancellationToken);
        ForEachPaged(count, 10, i => GetNodesInFolder(folder, i, 10, cancellationToken), foreachFn, cancellationToken);
    }

    public void DeleteDocumentAtPath(INodePath path, CancellationToken cancellationToken)
    {
        var doc = GetNodeAtPath(path, cancellationToken);
        DeleteDocument(doc as IReadOnlyDocument, cancellationToken);
    }

    private void DeleteDocument(IReadOnlyDocument doc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(doc);
        GetMetaOrThrowIfNotWritable(doc, writable =>
        {
            writable.DeletedDate = DateTime.UtcNow;
            writable.WriteIfChanged(cancellationToken: cancellationToken);
        });
    }

    public void DeleteDocumentsInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(folder);
        var count = GetCountOfNodesInFolder(folder, cancellationToken);
        ForEachNodeInFolder(folder, (doc) =>
        {
            DeleteDocument(doc as IReadOnlyDocument, cancellationToken);
        }, cancellationToken);
    }

    public IWritableDocument NewDocument(NewDocumentOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        var contentTypeExtension = GetExtensionFromContentType(options.contentType);
        if (contentTypeExtension.HasValue() && !contentTypeExtension.StartsWith('.'))
        {
            contentTypeExtension = $".{contentTypeExtension}";
        }
        var path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{(options.fileName ?? (Guid.NewGuid().ToString()))}{contentTypeExtension}");
        var doc = new WritableDocument(this, broadcastProviderResolver.MemCached, new DocPathRelative(path, config));
        var docMeta = doc.GetWritableMeta(cancellationToken);
        return doc;
    }

    public IWritableDocument CopyDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken)
    {
        var src = source.ConvertToAbsolutePath().PathValue;
        var dst = destination.ConvertToAbsolutePath().PathValue;
        System.IO.File.Copy(src, dst);
        System.IO.File.Copy($"{src}{Config.PathsConfig.DocMetaExtension}", $"{dst}{Config.PathsConfig.DocMetaExtension}");
        return GetNodeAtPath(destination, cancellationToken) as IWritableDocument;
    }

    public IWritableDocument MoveDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken)
    {
        var src = source.ConvertToAbsolutePath().PathValue;
        var dst = destination.ConvertToAbsolutePath().PathValue;
        System.IO.File.Move(src, dst);
        System.IO.File.Move($"{src}{Config.PathsConfig.DocMetaExtension}", $"{dst}{Config.PathsConfig.DocMetaExtension}");
        return GetNodeAtPath(destination, cancellationToken) as IWritableDocument;
    }
}