using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public interface IPathRuleService
{
    IPathRule[] Rules { get; }
    
    Task<IEnumerable<IReadOnlyNode>> FilterAndResolvePaths(IEnumerable<string> paths, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken);
}