

using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public class ContentTypePathsRule: IPathRule
{
    public Config Config { get; }

    public ContentTypePathsRule(Config config)
    {
        Config = config;
    }

    public Task<bool> IsApplicableToRequest(QueryDocumentOptions options)
    {
        return Task.FromResult(options.ContentType.HasValue());
    }

    public async Task<bool> IncludePath(string path, QueryDocumentOptions options, IReadOnlyDocumentResolver documentResolver, CancellationToken cancellationToken)
    {
        return options.ContentType == await documentResolver.GetContentTypeFromExtension(path.GetFullExtension());
    }
}