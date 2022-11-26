using slowpoke.core.Models.Node;

namespace slowpoke.core.Services.Node.Docs;


public interface IDocumentProviderResolver
{
    Task<ISlowPokeHost> Host { get; } // identifies the resolver

    bool IsForAutomatedTests { get; }
    
    Task<IReadOnlyDocumentResolver> ReadLocal { get; }

    Task<IWritableDocumentResolver> ReadWriteLocal { get; }

    Task<IEnumerable<IReadOnlyDocumentResolver>> ReadOnlyLocalDriveProviders { get; }

    Task<IEnumerable<IReadOnlyDocumentResolver>> ReadRemotes { get; }

    Task<IEnumerable<IWritableDocumentResolver>> ReadWriteRemotes { get; }

    Task<IEnumerable<IReadOnlyDocumentResolver>> AllReadonlyProviders { get; }
    
    Task<IEnumerable<IWritableDocumentResolver>> AllReadWriteProviders { get; }

    Task<NodePermissionCategories<IEnumerable<IReadOnlyDocumentResolver>>> ResolveReadable { get; }
    
    Task<NodePermissionCategories<IEnumerable<IWritableDocumentResolver>>> ResolveWritable { get; }

    Task<NodePermissionCategories<IReadOnlyDocumentResolver>> UnifiedReadable { get; }
    
    Task<NodePermissionCategories<IWritableDocumentResolver>> UnifiedWritable { get; }

    Task<IReadOnlyDocumentResolver> OpenReadRemote(Uri endpoint, TimeSpan? cacheDuration);

    Task<IWritableDocumentResolver> OpenReadWriteRemote(Uri endpoint, TimeSpan? cacheDuration);
}