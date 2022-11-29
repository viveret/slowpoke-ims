using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public class ExcludeCategoryPathsRule: IPathCategoryRule
{
    public ExcludeCategoryPathsRule()
    {
        
    }

    public Task<bool> IsApplicableToRequest(QueryDocumentOptions options)
    {
        return Task.FromResult(options.CategoriesToExclude != null && options.CategoriesToExclude.Length > 0);
    }

    public Task<bool> IncludePath(string path, FileCategory[] categories, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken)
    {
        if (!System.IO.File.Exists(path))
            return Task.FromResult(true); // skip non-files

        return Task.FromResult(
            categories.Intersect(options.CategoriesToExclude).Any()
        );
    }
}