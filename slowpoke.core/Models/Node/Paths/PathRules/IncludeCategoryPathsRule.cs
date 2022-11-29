using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public class IncludeCategoryPathsRule: IPathCategoryRule
{
    public IncludeCategoryPathsRule()
    {
        
    }

    public Task<bool> IsApplicableToRequest(QueryDocumentOptions options)
    {
        return Task.FromResult(options.CategoriesToIncludeOnly != null && options.CategoriesToIncludeOnly.Length > 0);
    }
    
    public Task<bool> IncludePath(string path, FileCategory[] categories, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken)
    {
        if (!System.IO.File.Exists(path))
            return Task.FromResult(true); // skip non-files

        return Task.FromResult(
            categories.Length == options.CategoriesToIncludeOnly.Length &&
            categories.Intersect(options.CategoriesToIncludeOnly).Count() == options.CategoriesToIncludeOnly.Length
        );
    }
}