using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
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

    
    public async Task<(string, IReadOnlyNode, ActionResult)> ValidateAndResolvePath(string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return (null, null, NotFound());
            //throw new ArgumentOutOfRangeException(nameof(path));
        }
        
        path = Uri.UnescapeDataString(path);

        try
        {
            var asDocPath = path.AsIDocPath(Config);
            var node = asDocPath != null ? await (await DocumentResolver.UnifiedReadable).CanRead.GetNodeAtPath(asDocPath, cancellationToken) : null;

            if (node == null || !await node.Exists)
            {
                return (path, null, NotFound());
            }

            return (path, node, null);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return (path, null, BadRequest(ex.Message));
        }
    }
}