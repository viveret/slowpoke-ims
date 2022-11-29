using System.Runtime.Serialization;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using slowpoke.core.Extensions;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs;


public class ReadOnlyDocumentMetaModel : IReadOnlyDocumentMeta
{
    public JsonObject MetaJson { get; set; } = new JsonObject();

    public ReadOnlyDocumentMetaModel() { }

    protected ReadOnlyDocumentMetaModel(IReadOnlyDocumentMeta other)
    {
        DocOrFolderPath = new NodePathModel(other.DocOrFolderPath);
        MetaPath = new NodePathModel(other.MetaPath);
        Title = other.Title;
        CreationDate = other.CreationDate;
        LastUpdate = other.LastUpdate;
        AccessDate = other.AccessDate;
        SyncEnabled = other.SyncEnabled;
        Favorited = other.Favorited;
        LastSyncDate = other.LastSyncDate;
        LastMetaUpdate = other.LastMetaUpdate;
        ArchivedDate = other.ArchivedDate;
        DeletedDate = other.DeletedDate;
        DocumentHash = other.DocumentHash;
        DocumentHashFunction = other.DocumentHashFunction;
    }

    public INodePath DocOrFolderPath { get; set; } = new NodePathModel();
    public INodePath MetaPath { get; set; } = new NodePathModel();

    public string Title { get; set; }

    [JsonIgnore, IgnoreDataMember]
    public Task<string> ContentType => Task.FromResult(ContentTypeSynch);
    
    public string ContentTypeSynch { get; set; }

    public DateTime CreationDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public DateTime AccessDate { get; set; }
    public bool SyncEnabled { get; set; }
    public bool Favorited { get; set; }
    public DateTime? LastSyncDate { get; set; }
    public DateTime? ArchivedDate { get; set; }

    public DateTime? DeletedDate { get; set; }
    public string DocumentHash { get; set; }
    public string DocumentHashFunction { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Always), IgnoreDataMember]
    public Task<bool> MetaExists => Task.FromResult(MetaExistsSynch);

    public bool MetaExistsSynch { get; set; }

    public DateTime LastMetaUpdate { get; set; }

    public Task<IReadOnlyDocument> GetDocument(CancellationToken cancellationToken) => throw new NotImplementedException();

    public string ComputeMetaHash() => throw new NotImplementedException();

    public string ComputeDocHash() => throw new NotImplementedException();

    public static async Task<ReadOnlyDocumentMetaModel> Create(IReadOnlyDocumentMeta readOnlyDocumentMeta)
    {
        var ret = new ReadOnlyDocumentMetaModel(readOnlyDocumentMeta);
        await ret.InitAsync(readOnlyDocumentMeta);
        return ret;
    }

    private async Task InitAsync(IReadOnlyDocumentMeta readOnlyDocumentMeta)
    {
        MetaExistsSynch = await readOnlyDocumentMeta.MetaExists;
        ContentTypeSynch = await readOnlyDocumentMeta.ContentType;
    }
}