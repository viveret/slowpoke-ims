using System.Data;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadWriteLocal;
using slowpoke.core.Services.Broadcast;
using SlowPokeIMS.Core.Services.Node.Docs;
using SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;

namespace slowpoke.core.Services.Node.Docs;


public class GenericReadWriteDocumentResolver : GenericReadOnlyDocumentResolver, IWritableDocumentResolver
{
    public override Task<NodePermissionCategories<bool>> Permissions
    {
        get => ExtendBasePermissions();
    }

    private async Task<NodePermissionCategories<bool>> ExtendBasePermissions()
    {
        var perms = await base.Permissions;
        perms.CanWrite = true;
        return perms;
    }

    public GenericReadWriteDocumentResolver(
        Config config, InMemoryGenericDocumentRepository inMemoryGenericDocumentRepository,
        IBroadcastProviderResolver broadcastProviderResolver) : base(config, broadcastProviderResolver, inMemoryGenericDocumentRepository)
    {
    }

    public override Task<IReadOnlyNode> GetNodeAtPath(INodePath path, CancellationToken cancellationToken)
    {
        //var docReadonly = base.GetNodeAtPath(path, cancellationToken);
        //if (Permissions.CanWrite) // docReadonly.Exists && 
        //{
            if (path.IsFolder)
            {
                return Task.FromResult<IReadOnlyNode>(new GenericWritableFolder(this, broadcastProvider, path));
            }
            else
            {
                return Task.FromResult<IReadOnlyNode>(new GenericWritableDocument(this, broadcastProvider, path));
            }
        //}
        //return docReadonly;
    }

    public override async Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        if ((await Permissions).CanWrite)
        {
            return new GenericWritableDocumentMeta(this, node.Path.ConvertToMetaPath());
        }
        else
        {
            return await base.GetMeta(node, cancellationToken);
        }
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
    public Task<Stream> OpenWrite(WritableDocument doc, CancellationToken cancellationToken) => inMemoryGenericDocumentRepository.OpenWriteInMemory(doc.Path, cancellationToken);

    public Task<Stream> OpenWriteMeta(IWritableDocumentMeta doc, CancellationToken cancellationToken) => inMemoryGenericDocumentRepository.OpenWriteInMemory(doc.MetaPath, cancellationToken);

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
        await DeleteDocument(doc as IReadOnlyDocument, cancellationToken);
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
            return DeleteDocument(doc as IReadOnlyDocument, cancellationToken);
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
        var doc = new GenericWritableDocument(this, broadcastProvider, new DocPathRelative(path, config));
        // var docMeta = doc.GetWritableMeta(cancellationToken);
        return doc;
    }

    public async Task<IWritableDocument> CopyDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken)
    {
        var src = source.ConvertToAbsolutePath().PathValue;
        var dst = destination.ConvertToAbsolutePath().PathValue;
        System.IO.File.Copy(src, dst);
        System.IO.File.Copy($"{src}{Config.PathsConfig.DocMetaExtension}", $"{dst}{Config.PathsConfig.DocMetaExtension}");
        return (await GetNodeAtPath(destination, cancellationToken)) as IWritableDocument;
    }

    public async Task<IWritableDocument> MoveDocumentTo(INodePath source, INodePath destination, CancellationToken cancellationToken)
    {
        var src = source.ConvertToAbsolutePath().PathValue;
        var dst = destination.ConvertToAbsolutePath().PathValue;
        System.IO.File.Move(src, dst);
        System.IO.File.Move($"{src}{Config.PathsConfig.DocMetaExtension}", $"{dst}{Config.PathsConfig.DocMetaExtension}");
        return (await GetNodeAtPath(destination, cancellationToken)) as IWritableDocument;
    }
}