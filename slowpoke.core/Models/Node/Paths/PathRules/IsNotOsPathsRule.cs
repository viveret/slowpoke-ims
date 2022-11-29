using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public class IsNotOsPathsRule: IPathRule
{
    public Config Config { get; }

    public IsNotOsPathsRule(Config config)
    {
        this.Config = config;
    }

    public Task<bool> IsApplicableToRequest(QueryDocumentOptions options)
    {
        return Task.FromResult(true);
    }

    public Task<bool> IncludePath(string path, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken)
    {
        return Task.FromResult(!Config.Paths.IsOS(path));
    }
}