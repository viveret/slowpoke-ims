using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public class SyncEnabledPathsRule: INodeMetaPathRule
{
    public Config Config { get; }

    public SyncEnabledPathsRule(Config config)
    {
        Config = config;
    }

    public Task<bool> IsApplicableToRequest(QueryDocumentOptions options)
    {
        return Task.FromResult(options.SyncEnabled.HasValue);
    }

    public Task<bool> IncludePath(IReadOnlyDocumentMeta meta, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken)
    {
        return Task.FromResult(meta.SyncEnabled == (options.SyncEnabled ?? false));
    }
}