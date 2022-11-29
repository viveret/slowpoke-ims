using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Collections;

namespace SlowPokeIMS.Core.Services.Node.Docs.PathRules;

public class GenericPathRuleService: IPathRuleService
{
    public GenericPathRuleService(
        Config config,
        IEnumerable<IPathRule> rules,
        IEnumerable<IPathCategoryRule> categoryRules,
        IEnumerable<INodeMetaPathRule> nodeMetaRules)
    {
        Config = config;
        Rules = rules.ToArray();
        CategoryRules = categoryRules.ToArray();
        NodeMetaRules = nodeMetaRules.ToArray();
    }

    public Config Config { get; }
    public IPathRule[] Rules { get; }
    public IPathCategoryRule[] CategoryRules { get; }
    public INodeMetaPathRule[] NodeMetaRules { get; }
    
    public async Task<IEnumerable<IReadOnlyNode>> FilterAndResolvePaths(IEnumerable<string> paths, QueryDocumentOptions options, IReadOnlyDocumentResolver docResolver, CancellationToken cancellationToken)
    {
        var applicableRules = (await Rules.WhereAsync(r => r.IsApplicableToRequest(options))).ToArray();
        var applicableCategoryRules = (await CategoryRules.WhereAsync(r => r.IsApplicableToRequest(options))).ToArray();
        var applicableNodeMetaRules = (await NodeMetaRules.WhereAsync(r => r.IsApplicableToRequest(options))).ToArray();

        var pathsFound = (await paths.WhereAsync(path => ShouldIncludePath(path, applicableRules, options, docResolver, cancellationToken))).ToArray();

        var pathCategories = paths.Select(p => (p, p.ToLower().Split('/')))
            .ToDictionary(k => k.p,
                            p =>
                            {
                                var categories = Config.Paths.FolderToCategoryMap
                                    .Where(folder => p.Item2.Contains(folder.Key))
                                    .Select(folder => folder.Value)
                                    .ToList();
                                if (Config.Paths.ExtensionToCategoryMap.TryGetValue(p.p.GetFullExtension(), out var cat))
                                {
                                    categories.Add(cat);
                                }
                                return categories.ToArray();
                            });

        var pathsByCategoryFound = (await pathsFound.WhereAsync(path => ShouldIncludePathByCategory(path, pathCategories, applicableCategoryRules, options, docResolver, cancellationToken))).ToArray();

        var nodesFound = (await Task.WhenAll(pathsByCategoryFound.Select(path => docResolver.GetNodeAtPath(path.AsIDocPath(Config).ConvertToAbsolutePath(), cancellationToken)))).ToArray();
        var nodeMetaFound = (await nodesFound.WhereAsync(node => ShouldIncludePathByNodeMeta(node, applicableNodeMetaRules, options, docResolver, cancellationToken))).ToArray();

        return nodeMetaFound;
    }

    private async Task<bool> ShouldIncludePathByNodeMeta(IReadOnlyNode node, INodeMetaPathRule[] NodeMetaRules, QueryDocumentOptions options, IReadOnlyDocumentResolver docResolver, CancellationToken cancellationToken)
    {
        var meta = await docResolver.GetMeta(node, cancellationToken);
        return (await NodeMetaRules.WhereAsync(r => r.IncludePath(meta, options, docResolver, cancellationToken))).Count() == NodeMetaRules.Length;
    }

    private async Task<bool> ShouldIncludePathByCategory(string path, Dictionary<string, FileCategory[]> pathCategories, IPathCategoryRule[] CategoryRules, QueryDocumentOptions options, IReadOnlyDocumentResolver docResolver, CancellationToken cancellationToken)
    {
        var categories = pathCategories[path];
        return (await CategoryRules.WhereAsync(r => r.IncludePath(path, categories, options, docResolver, cancellationToken))).Count() == CategoryRules.Length;
    }

    private async Task<bool> ShouldIncludePath(string arg, IPathRule[] Rules, QueryDocumentOptions options, IReadOnlyDocumentResolver docResolver, CancellationToken cancellationToken)
    {
        return (await Rules.WhereAsync(r => r.IncludePath(arg, options, docResolver, cancellationToken))).Count() == Rules.Length;
    }

    public static GenericPathRuleService Empty(Config cfg)
    {
        return new GenericPathRuleService(cfg, Enumerable.Empty<IPathRule>(), Enumerable.Empty<IPathCategoryRule>(), Enumerable.Empty<INodeMetaPathRule>());
    }
}