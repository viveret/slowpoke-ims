using System.Text.Json.Nodes;

namespace slowpoke.core.Models.Node.Docs;

public interface IReadOnlyDocumentMeta
{
    INodePath DocOrFolderPath { get; }
    INodePath MetaPath { get; }
    Task<bool> MetaExists { get; }
    bool Favorited { get; }
    bool SyncEnabled { get; }
    string Title { get; }
    Task<string> ContentType { get; }
    DateTime CreationDate { get; }
    DateTime AccessDate { get; }
    DateTime LastUpdate { get; }
    DateTime LastMetaUpdate { get; }
    DateTime? ArchivedDate { get; }
    DateTime? DeletedDate { get; }
    DateTime? LastSyncDate { get; }
    string DocumentHash { get; }
    string DocumentHashFunction { get; }

    JsonObject MetaJson { get; }

    Task<IReadOnlyDocument> GetDocument(CancellationToken cancellationToken);
    
    string ComputeMetaHash();
}
