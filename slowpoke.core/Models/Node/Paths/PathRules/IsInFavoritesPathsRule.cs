using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public class IsInFavoritesPathsRule: INodeMetaPathRule
{
    public Task<bool> IsApplicableToRequest(QueryDocumentOptions options)
    {
        return Task.FromResult(options.IsInFavorites.HasValue);
    }

    public Task<bool> IncludePath(IReadOnlyDocumentMeta meta, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken)
    {
        return Task.FromResult(meta.Favorited == (options.IsInFavorites ?? false));
    }
}