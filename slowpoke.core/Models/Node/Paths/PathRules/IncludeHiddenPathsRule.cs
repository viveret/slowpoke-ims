

using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public class IncludeHiddenPathsRule: IPathRule
{
    public Task<bool> IsApplicableToRequest(QueryDocumentOptions options)
    {
        return Task.FromResult(!options.IncludeHidden);
    }

    public Task<bool> IncludePath(string path, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken)
    {
        if (System.IO.Path.HasExtension(path))
        {
            return Task.FromResult(!System.IO.Path.GetFileName(path).StartsWith('.'));
        }
        else
        {
            var dir = System.IO.Path.GetDirectoryName(path);
            return Task.FromResult(dir != null ? !dir.StartsWith('.') : !path.StartsWith("/."));
        }
    }
}