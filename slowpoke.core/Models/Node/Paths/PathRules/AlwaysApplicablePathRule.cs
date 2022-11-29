

using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public class AlwaysApplicablePathRule: IPathRule
{
    public Task<bool> IsApplicableToRequest(QueryDocumentOptions options)
    {
        return Task.FromResult(true);
    }

    public Task<bool> IncludePath(string path, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}