using System.Data;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadWriteLocal;
using slowpoke.core.Services.Broadcast;

namespace slowpoke.core.Services.Node.Docs;


public class WritableLocalDocumentResolver : ReadOnlyLocalDocumentResolver, IWritableDocumentResolver
{
    public override Task<NodePermissionCategories<bool>> Permissions
    {
        get => ExtendBasePermsAsync();
    }

    private async Task<NodePermissionCategories<bool>> ExtendBasePermsAsync()
    {
        var perms = await base.Permissions;
        perms.CanWrite = true;
        return perms;
    }

    public WritableLocalDocumentResolver(Config config, IBroadcastProviderResolver broadcastProviderResolver) : base(config, broadcastProviderResolver)
    {
    }

    public override async Task<IReadOnlyNode> GetNodeAtPath(INodePath path, CancellationToken cancellationToken)
    {
        var docReadonly = await base.GetNodeAtPath(path, cancellationToken);
        if (await docReadonly.Exists && (await Permissions).CanWrite)
        {
            return new WritableDocument(this, broadcastProviderResolver.MemCached, path);
        }
        return docReadonly;
    }

    public override async Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        var metaReadonly = await base.GetMeta(node, cancellationToken);
        if ((await Permissions).CanWrite)
        {
            return new WritableDocumentMeta(this, metaReadonly.Path);
        }
        return metaReadonly;
    }

    public async Task ArchiveDocumentAtPath(INodePath path, CancellationToken cancellationToken)
    {
        var doc = await GetNodeAtPath(path, cancellationToken);
        await ArchiveNode(doc, cancellationToken);
    }

    private Task ArchiveNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        if (node is IReadOnlyDocument doc)
        {
            return ArchiveDocument(doc, cancellationToken);
        }
        else if (node is IReadOnlyFolder folder)
        {
            return ArchiveDocumentsInFolder(folder.Path, cancellationToken);
        }
        else
        {
            throw new NotSupportedException($"Node type {node.GetType().Name} not supported");
        }
    }

    private Task ArchiveDocument(IReadOnlyDocument doc, CancellationToken cancellationToken)
    {
        return GetMetaOrThrowIfNotWritable(doc, writable =>
        {
            writable.ArchivedDate = DateTime.UtcNow;
            return writable.WriteIfChanged(cancellationToken: cancellationToken);
        });
    }

    // todo: fix this, if client calls with using() then the stream will be closed prematurely
    public Task<Stream> OpenWrite(WritableDocument doc, CancellationToken cancellationToken) => OpenWriteLocal(doc.Path, cancellationToken);

    public Task<Stream> OpenWriteMeta(IWritableDocumentMeta doc, CancellationToken cancellationToken) => OpenWriteLocal(doc.Path.ConvertToAbsolutePath().ConvertToMetaPath(), cancellationToken);

    private static Task<Stream> OpenWriteLocal(INodePath path, CancellationToken cancellationToken)
    {
        var p = path.ConvertToAbsolutePath().PathValue;
        if (string.IsNullOrWhiteSpace(p))
            throw new ArgumentNullException(nameof(path));
        else if (File.Exists(p))
            return Task.FromResult<Stream>(File.Open(p, FileMode.Create));
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
            return Task.FromResult<Stream>(File.Create(p));
        }
    }

    private Task GetMetaOrThrowIfNotWritable(IReadOnlyDocument doc, Func<IWritableDocumentMeta, Task> action)
    {
        var meta = doc.Meta;
        if (meta is IWritableDocumentMeta writable)
        {
            return action(writable);
        }
        else
        {
            throw new ReadOnlyException();
        }
    }

    public Task ArchiveDocumentsInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        return ForEachNodeInFolder(folder, (doc) =>
        {
            return ArchiveDocument((IReadOnlyDocument)doc, cancellationToken);
        }, cancellationToken);
    }

    private async Task ForEachPaged<T>(int count, int pageSize, Func<int, Task<IEnumerable<T>>> getter, Func<T, Task> foreachFn, CancellationToken cancellationToken)
    {
        for (int i = 0; i < count; i += pageSize)
        {
            foreach (var doc in await getter(i))
            {
                await foreachFn(doc);
            }
        }
    }

    private async Task ForEachNodeInFolder(INodePath folder, Func<IReadOnlyNode, Task> foreachFn, CancellationToken cancellationToken)
    {
        var count = await GetCountOfNodesInFolder(folder, cancellationToken);
        await ForEachPaged(count, 10, i => GetNodesInFolder(folder, i, 10, cancellationToken), foreachFn, cancellationToken);
    }

    public async Task DeleteDocumentAtPath(INodePath path, CancellationToken cancellationToken)
    {
        var doc = await GetNodeAtPath(path, cancellationToken);
        await DeleteDocument((IReadOnlyDocument) doc, cancellationToken);
    }

    private Task DeleteDocument(IReadOnlyDocument doc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(doc);
        return GetMetaOrThrowIfNotWritable(doc, writable =>
        {
            writable.DeletedDate = DateTime.UtcNow;
            return writable.WriteIfChanged(cancellationToken: cancellationToken);
        });
    }

    public async Task DeleteDocumentsInFolder(INodePath folder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(folder);
        var count = await GetCountOfNodesInFolder(folder, cancellationToken);
        await ForEachNodeInFolder(folder, (doc) =>
        {
            return DeleteDocument((IReadOnlyDocument) doc, cancellationToken);
        }, cancellationToken);
    }

    public async Task<IWritableDocument> NewDocument(NewDocumentOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        var contentTypeExtension = await GetExtensionFromContentType(options.contentType);
        if (contentTypeExtension.HasValue() && !contentTypeExtension.StartsWith('.'))
        {
            contentTypeExtension = $".{contentTypeExtension}";
        }
        var path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{(options.fileName ?? (Guid.NewGuid().ToString()))}{contentTypeExtension}");
        var doc = new WritableDocument(this, broadcastProviderResolver.MemCached, new DocPathRelative(path, config));
        var docMeta = await doc.GetWritableMeta(cancellationToken);
        return doc;
    }

    public async Task<IWritableDocument> CopyDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken)
    {
        var src = source.ConvertToAbsolutePath().PathValue;
        var dst = destination.ConvertToAbsolutePath().PathValue;
        System.IO.File.Copy(src, dst);
        System.IO.File.Copy($"{src}{Config.PathsConfig.DocMetaExtension}", $"{dst}{Config.PathsConfig.DocMetaExtension}");
        return (IWritableDocument) (await GetNodeAtPath(destination, cancellationToken));
    }

    public async Task<IWritableDocument> MoveDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken)
    {
        var src = source.ConvertToAbsolutePath().PathValue;
        var dst = destination.ConvertToAbsolutePath().PathValue;
        System.IO.File.Move(src, dst);
        System.IO.File.Move($"{src}{Config.PathsConfig.DocMetaExtension}", $"{dst}{Config.PathsConfig.DocMetaExtension}");
        return (IWritableDocument) (await GetNodeAtPath(destination, cancellationToken));
    }
}