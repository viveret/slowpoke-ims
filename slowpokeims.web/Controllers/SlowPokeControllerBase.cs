using System;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Web.Controllers;


public class SlowPokeControllerBase: Controller
{
    public SlowPokeControllerBase(
        IDocumentProviderResolver documentProviderResolver, 
        Config config)
    {
        DocumentResolver = documentProviderResolver;
        Config = config;
    }

    public IDocumentProviderResolver DocumentResolver { get; }
    public Config Config { get; }

    
    protected (string, IReadOnlyNode, ActionResult) ValidateAndResolvePath(string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return (null, null, NotFound());
        }
        
        path = Uri.UnescapeDataString(path);
        var node = DocumentResolver.UnifiedReadable.CanRead.GetNodeAtPath(path.AsIDocPath(Config), cancellationToken);
        if (node == null || !node.Exists)
        {
            return (path, null, NotFound());
        }

        return (path, node, null);
    }
}