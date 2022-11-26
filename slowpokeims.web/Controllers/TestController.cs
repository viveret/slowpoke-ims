using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;
using SlowPokeIMS.Web.Controllers.Attributes;

namespace SlowPokeIMS.Web.Controllers;


[ShowInNavBar("Test")]
public class TestController : SlowPokeControllerBase
{
    public TestController(
        IDocumentProviderResolver documentResolver,
        //IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver, config)
    {
    }

    [HttpGet("test")]
    public ActionResult Index() => View();

    [HttpGet("test/create-folder/{path}/{syncEnabled}")]
    public Task<ActionResult> CreateFolder(string path, string syncEnabled) =>
        RunUsingGenericReadWriteResolverIfAutomatedTest(async genericResolver =>
        {
            path = Uri.UnescapeDataString(path);
            genericResolver.inMemoryGenericDocumentRepository.CreateFolder(path);
            var node = await genericResolver.GetNodeAtPath(path.AsIDocPath(Config), CancellationToken.None);
            if (syncEnabled?.ToLower() == "true")
            {
                await node.TurnOnSync(CancellationToken.None);
            }
        });

    [HttpGet("test/create-file/{path}/{syncEnabled}")]
    public Task<ActionResult> CreateFile(string path, string syncEnabled) =>
        RunUsingGenericReadWriteResolverIfAutomatedTest(async genericResolver =>
        {
            path = Uri.UnescapeDataString(path);
            genericResolver.inMemoryGenericDocumentRepository.CreateFile(path);
            var node = await genericResolver.GetNodeAtPath(path.AsIDocPath(Config), CancellationToken.None);
            if (syncEnabled?.ToLower() == "true")
            {
                await node.TurnOnSync(CancellationToken.None);
            }
        });

    [HttpGet("test/EnsureNoFilesOrFolders")]
    public Task<ActionResult> EnsureNoFilesOrFolders() =>
        RunUsingGenericReadOnlyResolverIfAutomatedTest(genericResolver =>
        {
            genericResolver.inMemoryGenericDocumentRepository.Files.Forward.Clear();
            genericResolver.inMemoryGenericDocumentRepository.Folders.Forward.Clear();
            return Task.CompletedTask;
        });

    private async Task<ActionResult> RunUsingGenericReadOnlyResolverIfAutomatedTest(Func<GenericReadOnlyDocumentResolver, Task> actionContext)
    {
        var rl = await DocumentResolver.ReadLocal;
        if (rl is GenericReadOnlyDocumentResolver genericResolver)
        {
            if (genericResolver.inMemoryGenericDocumentRepository != null)
            {
                await actionContext(genericResolver);
                return Ok("true");
            }
            else
            {
                return Ok($"genericResolver.inMemoryGenericDocumentRepository is null");
            }
        }
        else
        {
            return Ok($"Invalid type {rl.GetType().Name} for resolved document provider");
        }
    }

    private async Task<ActionResult> RunUsingGenericReadWriteResolverIfAutomatedTest(Func<GenericReadWriteDocumentResolver, Task> actionContext)
    {
        var rwl = await DocumentResolver.ReadWriteLocal;
        if (rwl is GenericReadWriteDocumentResolver genericResolver)
        {
            if (genericResolver.inMemoryGenericDocumentRepository != null)
            {
                await actionContext(genericResolver);
                return Ok("true");
            }
            else
            {
                return Ok($"genericResolver.inMemoryGenericDocumentRepository is null");
            }
        }
        else
        {
            return Ok($"Invalid type {rwl.GetType().Name} for resolved document provider");
        }
    }

    // private ActionResult RunUsingTypeResolverIfAutomatedTest<T>(Action<T> actionContext)
    // {
    //     if (DocumentResolver.ReadWriteLocal is T genericResolver)
    //     {
    //         if (genericResolver.inMemoryGenericDocumentRepository != null)
    //         {
    //             actionContext(genericResolver);
    //             return Ok("true");
    //         }
    //         else
    //         {
    //             return Ok($"genericResolver.inMemoryGenericDocumentRepository is null");
    //         }
    //     }
    //     else
    //     {
    //         return Ok($"Invalid type {DocumentResolver.ReadLocal.GetType().Name} for resolved document provider");
    //     }
    // }
}