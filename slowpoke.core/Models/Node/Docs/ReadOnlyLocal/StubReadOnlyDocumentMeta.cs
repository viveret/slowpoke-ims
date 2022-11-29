using System.Text.Json.Nodes;
using slowpoke.core.Extensions;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs.ReadOnlyLocal;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs;


public class StubReadOnlyDocumentMeta : IReadOnlyDocumentMeta
{
    private readonly Config config;

    public JsonObject MetaJson { get; private set; }

    public StubReadOnlyDocumentMeta(Config config, INodePath path)
    {
        this.config = config;
        DocOrFolderPath = path ?? throw new ArgumentNullException(nameof(path));
        MetaPath = path.ConvertToMetaPath();
        MetaJson = new JsonObject();
    }

    public INodePath DocOrFolderPath { get; private set; }
    public INodePath MetaPath { get; private set; }

    public string Title { get => MetaJson.TryGetPropertyValue(nameof(Title), out var v) && v != null ? v.GetValue<string>() : string.Empty; }
    public Task<string> ContentType { get => Task.FromResult(string.Empty); }
    public DateTime CreationDate { get => DateTime.MinValue; }
    public DateTime LastUpdate { get => DateTime.MinValue; }
    public DateTime AccessDate { get => DateTime.MinValue; }
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
    public Task<bool> MetaExists => Task.FromResult(true);

    public DateTime LastMetaUpdate => DateTime.MinValue;

    public Task<IReadOnlyDocument> GetDocument(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyDocument>(new StubReadOnlyDocument(true, DocOrFolderPath, config));

    public string ComputeMetaHash() => string.Empty;

    public string ComputeDocHash() => string.Empty;
}