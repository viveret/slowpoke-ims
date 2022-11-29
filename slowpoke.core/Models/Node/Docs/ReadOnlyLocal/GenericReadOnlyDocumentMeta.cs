using System.Text.Json.Nodes;
using slowpoke.core.Extensions;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Services.Node.Docs;
using SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;

namespace slowpoke.core.Models.Node.Docs;


public class GenericReadOnlyDocumentMeta : IReadOnlyDocumentMeta
{
    private readonly IReadOnlyDocumentResolver documentResolver;
    private readonly InMemoryGenericDocumentRepository repo;

    public JsonObject MetaJson { get; private set; }

    public GenericReadOnlyDocumentMeta(GenericReadOnlyDocumentResolver documentResolver, INodePath path)
    {
        this.documentResolver = documentResolver ?? throw new ArgumentNullException(nameof(documentResolver));
        DocOrFolderPath = path?.ConvertToMetaPath() ?? throw new ArgumentNullException(nameof(path));
        MetaPath = DocOrFolderPath.ConvertToMetaPath();

        this.repo = documentResolver.inMemoryGenericDocumentRepository;

        if (repo.TryGetMetaForPath(path.PathValue, out var jsonObject) && jsonObject != null)
        {
            MetaJson = jsonObject;
        }
        else
        {
            MetaJson = new JsonObject();
        }
    }

    public INodePath DocOrFolderPath { get; private set; }
    public INodePath MetaPath { get; private set; }

    public string Title { get => MetaJson.TryGetPropertyValue(nameof(Title), out var v) && v != null ? v.GetValue<string>() : string.Empty; }
    public Task<string> ContentType { get => documentResolver.GetContentTypeFromExtension(DocOrFolderPath.PathValue.GetFullExtension()); }
    public DateTime CreationDate { get => repo.GetOsLevelMeta(DocOrFolderPath).CreationTimeUtc; }
    public DateTime LastUpdate { get => repo.GetOsLevelMeta(DocOrFolderPath).LastWriteTimeUtc; }
    public DateTime AccessDate { get => repo.GetOsLevelMeta(DocOrFolderPath).LastAccessTimeUtc; }
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
    public Task<bool> MetaExists => GetMetaExistsAsync();
    private async Task<bool> GetMetaExistsAsync() => await documentResolver.HasMeta(await GetDocument(CancellationToken.None), CancellationToken.None);

    public DateTime LastMetaUpdate => repo.GetOsLevelMeta(MetaPath).MetaLastWriteTimeUtc;

    public async Task<IReadOnlyDocument> GetDocument(CancellationToken cancellationToken) => (IReadOnlyDocument) (await documentResolver.GetNodeAtPath(DocOrFolderPath, cancellationToken));

    public string ComputeMetaHash()
    {
        using var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(MetaJson.ToString()));
        return stream.ComputeMD5FromStream(false);
    }
    
    public string ComputeDocHash()
    {
        using var stream = new MemoryStream(repo.GetFileData(DocOrFolderPath));
        return stream.ComputeMD5FromStream(false);
    }
}