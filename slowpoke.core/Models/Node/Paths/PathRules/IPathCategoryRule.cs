using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public interface IPathCategoryRule
{
    Task<bool> IsApplicableToRequest(QueryDocumentOptions options);
    Task<bool> IncludePath(string path, FileCategory[] categories, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken);
}