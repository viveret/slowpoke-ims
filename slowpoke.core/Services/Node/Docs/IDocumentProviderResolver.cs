using slowpoke.core.Models.Node;

namespace slowpoke.core.Services.Node.Docs;


public interface IDocumentProviderResolver
{
    ISlowPokeHost Host { get; } // identifies the resolver
    
    IReadOnlyDocumentResolver ReadLocal { get; }

    IWritableDocumentResolver ReadWriteLocal { get; }

    IEnumerable<IReadOnlyDocumentResolver> ReadOnlyLocalDriveProviders { get; }

    IEnumerable<IReadOnlyDocumentResolver> ReadRemotes { get; }

    IEnumerable<IWritableDocumentResolver> ReadWriteRemotes { get; }

    IEnumerable<IReadOnlyDocumentResolver> AllReadonlyProviders { get; }
    
    IEnumerable<IWritableDocumentResolver> AllReadWriteProviders { get; }

    NodePermissionCategories<IEnumerable<IReadOnlyDocumentResolver>> ResolveReadable { get; }
    
    NodePermissionCategories<IEnumerable<IWritableDocumentResolver>> ResolveWritable { get; }

    NodePermissionCategories<IReadOnlyDocumentResolver> UnifiedReadable { get; }
    
    NodePermissionCategories<IWritableDocumentResolver> UnifiedWritable { get; }

    IReadOnlyDocumentResolver OpenReadRemote(Uri endpoint, TimeSpan? cacheDuration);

    IWritableDocumentResolver OpenReadWriteRemote(Uri endpoint, TimeSpan? cacheDuration);
}