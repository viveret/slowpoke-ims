using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public interface IPathRule
{
    Task<bool> IsApplicableToRequest(QueryDocumentOptions options);
    Task<bool> IncludePath(string path, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken);
}