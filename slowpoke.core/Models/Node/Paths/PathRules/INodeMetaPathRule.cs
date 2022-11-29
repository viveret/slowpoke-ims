using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public interface INodeMetaPathRule
{
    Task<bool> IsApplicableToRequest(QueryDocumentOptions options);
    Task<bool> IncludePath(IReadOnlyDocumentMeta meta, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken);
}