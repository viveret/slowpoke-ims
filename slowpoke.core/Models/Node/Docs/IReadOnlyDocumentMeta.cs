using System.Text.Json.Nodes;

namespace slowpoke.core.Models.Node.Docs;

public interface IReadOnlyDocumentMeta
{
    INodePath Path { get; }
    bool MetaExists { get; }
    bool Favorited { get; }
    bool SyncEnabled { get; }
    string Title { get; }
    string ContentType { get; }
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

    IReadOnlyDocument GetDocument(CancellationToken cancellationToken);
    
    string ComputeMetaHash();
}
