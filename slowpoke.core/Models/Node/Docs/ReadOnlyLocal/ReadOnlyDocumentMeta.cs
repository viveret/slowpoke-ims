using System.Text.Json.Nodes;
using slowpoke.core.Extensions;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs;


public class ReadOnlyDocumentMeta : IReadOnlyDocumentMeta
{
    private IReadOnlyDocumentResolver documentResolver;

    protected FileInfo FileInfo { get; private set; }
    
    protected FileInfo MetaFileInfo { get; private set; }
    
    public JsonObject MetaJson { get; private set; }

    public ReadOnlyDocumentMeta(IReadOnlyDocumentResolver docResolver, INodePath path)
    {
        this.documentResolver = docResolver ?? throw new ArgumentNullException(nameof(docResolver));
        Path = path ?? throw new ArgumentNullException(nameof(path));
        var absPath = path.ConvertToAbsolutePath();

        if (string.IsNullOrWhiteSpace(absPath.PathValue))
        {
            throw new ArgumentException($"Invalid path value '{path.PathValue}'", nameof(path));
        }

        FileInfo = new FileInfo(absPath.PathValue);
        MetaFileInfo = new FileInfo(absPath.ConvertToMetaPath().PathValue);
        if (MetaFileInfo.Exists)
        {
            using var reader = new StreamReader(MetaFileInfo.OpenRead(), System.Text.Encoding.UTF8);
            var str = reader.ReadToEnd();
            MetaJson = (JsonObject.Parse(str) as JsonObject)!;
        }
        else
        {
            MetaJson = new JsonObject();
        }
    }

    public INodePath Path { get; private set; }

    public string Title { get => MetaJson.TryGetPropertyValue(nameof(Title), out var v) && v != null ? v.GetValue<string>() : string.Empty; }
    public Task<string> ContentType { get => documentResolver.GetContentTypeFromExtension(FileInfo.FullName.GetFullExtension()); }
    public DateTime CreationDate { get => FileInfo.CreationTimeUtc; }
    public DateTime LastUpdate { get => FileInfo.LastWriteTimeUtc; }
    public DateTime AccessDate { get => FileInfo.LastAccessTimeUtc; }
    public bool SyncEnabled { get => MetaJson.TryGetPropertyValue(nameof(SyncEnabled), out var b) && b != null ? b.GetValue<bool>() : false; }
    public bool Favorited { get => MetaJson.TryGetPropertyValue(nameof(Favorited), out var b) && b != null ? b.GetValue<bool>() : false; }
    public DateTime? LastSyncDate { get => TryGetDateTime(nameof(LastSyncDate), out var dt) ? dt : null; }
    public DateTime? ArchivedDate { get => TryGetDateTime(nameof(ArchivedDate), out var dt) ? dt : null; }

    private bool TryGetDateTime(string name, out DateTime dt)
    {
        var ret = MetaJson.TryGetPropertyValue(name, out var v);
        if (ret && v != null)
        {
            dt = DateTime.Parse(v.ToString());
        }
        else
        {
            dt = DateTime.MinValue;
        }
        return ret;
    }

    public DateTime? DeletedDate { get => TryGetDateTime(nameof(DeletedDate), out var dt) ? dt : null; }
    public string DocumentHash => ComputeDocHash();
    public string DocumentHashFunction => "MD5";
    public Task<bool> MetaExists => Task.FromResult(MetaFileInfo.Exists);

    public DateTime LastMetaUpdate => MetaFileInfo.LastWriteTimeUtc;

    public async Task<IReadOnlyDocument> GetDocument(CancellationToken cancellationToken) => (IReadOnlyDocument) (await documentResolver.GetNodeAtPath(Path, cancellationToken));

    public string ComputeMetaHash()
    {
        using var stream = MetaFileInfo.OpenRead();
        return stream.ComputeMD5FromStream(false);
    }
    
    public string ComputeDocHash()
    {
        using var stream = FileInfo.OpenRead();
        return stream.ComputeMD5FromStream(false);
    }
}