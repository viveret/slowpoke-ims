using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Controllers;
using SlowPokeIMS.Web.Controllers.Attributes;
using SlowPokeIMS.Web.Extensions;
using SlowPokeIMS.Web.ViewModels.AnyTypeDoc;

namespace SlowPokeIMS.Web.Controllers;

[DocController]
public class AnyTypeDocController: SlowPokeControllerBase
{
    public AnyTypeDocController(IDocumentProviderResolver documentProviderResolver, Config config): base(documentProviderResolver, config)
    {
    }

    [DocAction, HttpGet("doc/copy/{path}")]
    public async Task<ActionResult> Copy(string path, CancellationToken cancellationToken)
    {
        var doc = await ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;
        
        return View(new CopyViewModel { path = doc.Item1, DestinationDefault = await GetDestinationDefault(doc.Item1, cancellationToken), CancelUrl = Request.Headers.Referer });
    }

    [HttpPost("doc/copy/{path}")]
    public async Task<ActionResult> Copy(string path, string Destination, CancellationToken cancellationToken)
    {
        return await TryActionOnDocument(path, async (INodePath path, IReadOnlyDocument doc) =>
        {
            var destDefault = await GetDestinationDefault(path.PathValue, cancellationToken);
            var dest = string.IsNullOrWhiteSpace(Destination) ? destDefault : (Destination = Uri.UnescapeDataString(Destination));
            var destPath = dest.AsIDocPath(Config);
            if (await (await DocumentResolver.UnifiedReadable).CanRead.NodeExistsAtPath(destPath, cancellationToken))
            {
                ModelState.AddModelError(nameof(Destination), "Document already exists at destination");
            }

            if (ModelState.IsValid)
            {
                await (await DocumentResolver.UnifiedWritable).CanWrite.CopyDocumentTo(path, destPath, cancellationToken);
            }
        }, cancellationToken, () => Task.FromResult<ActionResult>(RedirectToAction(nameof(HomeController.Details), "Home", new { path = Destination })),
            async (string path) => View(new CopyViewModel { path = path, Destination = Destination, DestinationDefault = await GetDestinationDefault(path, cancellationToken), CancelUrl = Request.Headers.Referer }));
    }

    private async Task<string> GetDestinationDefault(string path, CancellationToken cancellationToken)
    {
        var ext = path.GetFullExtension();
        var newPath = path.Substring(0, path.Length - ext.Length) + "(copy)" + ext;
        var numTries = 1;
        var ur = await DocumentResolver.UnifiedReadable;
        while (await ur.CanRead.NodeExistsAtPath(newPath.AsIDocPath(Config), cancellationToken))
        {
            numTries++;
            newPath = path.Substring(0, path.Length - ext.Length) + $"(copy {numTries})" + ext;
        }
        return newPath;
    }

    [DocAction, HttpGet("doc/move/{path}")]
    public async Task<ActionResult> Move(string path, CancellationToken cancellationToken)
    {
        var doc = await this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;
        
        return View(new MoveViewModel { path = doc.Item1, CancelUrl = Request.Headers.Referer });
    }

    [HttpPost("doc/move/{path}")]
    public Task<ActionResult> Move(string path, string Destination, CancellationToken cancellationToken)
    {
        return TryActionOnDocument(path, async (INodePath path, IReadOnlyDocument doc) =>
        {
            Destination = Uri.UnescapeDataString(Destination);
            var destPath = Destination.AsIDocPath(Config);
            var ur = await DocumentResolver.UnifiedReadable;
            if (await ur.CanRead.NodeExistsAtPath(destPath, cancellationToken))
            {
                ModelState.AddModelError(nameof(Destination), "Document already exists at destination");
            }

            if (ModelState.IsValid)
            {
                await (await DocumentResolver.UnifiedWritable).CanWrite.MoveDocumentTo(path, destPath, cancellationToken);
            }
        }, cancellationToken, () => Task.FromResult<ActionResult>(RedirectToAction(nameof(HomeController.Details), "Home", new { path = Destination })),
            (string path) => Task.FromResult<ActionResult>(View(new MoveViewModel { path = path, Destination = Destination, CancelUrl = Request.Headers.Referer })));
    }

    [DocAction, HttpGet("doc/archive/{path}"), HttpPost("doc/archive/{path}")]
    public async Task<ActionResult> Archive(string path, CancellationToken cancellationToken)
    {
        return await TryActionOnDocument(path, async (INodePath path, IReadOnlyDocument doc) => await (await DocumentResolver.UnifiedWritable).CanWrite.ArchiveDocumentAtPath(path, cancellationToken), cancellationToken);
    }

    [DocAction, HttpGet("doc/delete/{path}"), HttpPost("doc/delete/{path}")]
    public Task<ActionResult> Delete(string path, CancellationToken cancellationToken)
    {
        return TryActionOnDocument(path, async (INodePath path, IReadOnlyDocument doc) => await (await DocumentResolver.UnifiedWritable).CanWrite.DeleteDocumentAtPath(path, cancellationToken), cancellationToken);
    }

    [DocAction(inlineAction: true), DocActionLabel("Turn on Sync"), HttpPost("doc/turn-off-sync/{path}")]
    [DocActionVisibility(typeof(AnyTypeDocController), nameof(ShouldShowTurnOffSync))]
    public async Task<ActionResult> TurnOffSync(string path, CancellationToken cancellationToken)
    {
        var doc = await this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;

        var node = await (await DocumentResolver.UnifiedWritable).CanWrite.GetNodeAtPath(doc.Item1.AsIDocPath(Config), cancellationToken);
        await node.TurnOffSync(cancellationToken);

        return View("TurnOnSyncButton");
    }

    public static async Task<bool> ShouldShowTurnOffSync(IReadOnlyNode node, IServiceProvider services)
    {
        return await node.CanSync && (await node.Meta).SyncEnabled;
    }

    [DocAction(inlineAction: true), DocActionLabel("Turn on Sync"), HttpPost("doc/turn-on-sync/{path}")]
    [DocActionVisibility(typeof(AnyTypeDocController), nameof(ShouldShowTurnOnSync))]
    public async Task<ActionResult> TurnOnSync(string path, CancellationToken cancellationToken)
    {
        var doc = await this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;

        var node = await (await DocumentResolver.UnifiedWritable).CanWrite.GetNodeAtPath(doc.Item1.AsIDocPath(Config), cancellationToken);
        await node.TurnOnSync(cancellationToken);

        return View("TurnOffSyncButton");
    }

    public static async Task<bool> ShouldShowTurnOnSync(IReadOnlyNode node, IServiceProvider services)
    {
        return await node.CanSync && !(await node.Meta).SyncEnabled;
    }

    [DocAction(inlineAction: true), DocActionLabel("Broadcast Changes"), HttpPost("doc/broadcast-changes/{path}")]
    [DocActionVisibility(typeof(AnyTypeDocController), nameof(ShouldShowBroadcastChanges))]
    public async Task<ActionResult> BroadcastChanges(string path, CancellationToken cancellationToken)
    {
        var doc = await this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;

        var node = await (await DocumentResolver.UnifiedWritable).CanWrite.GetNodeAtPath(doc.Item1.AsIDocPath(Config), cancellationToken);
        await node.BroadcastChanges(cancellationToken);

        return View("BroadcastChangesButton", path);
    }

    public static async Task<bool> ShouldShowBroadcastChanges(IReadOnlyNode node, IServiceProvider services)
    {
        return await node.CanSync && (await node.Meta).SyncEnabled;
    }

    [DocAction(inlineAction: true), DocActionLabel("Poll For Changes"), HttpPost("doc/poll-for-changes/{path}")]
    [DocActionVisibility(typeof(AnyTypeDocController), nameof(ShouldShowPollForChanges))]
    public async Task<ActionResult> PollForChanges(string path, CancellationToken cancellationToken)
    {
        var doc = await this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;

        var node = await (await DocumentResolver.UnifiedWritable).CanWrite.GetNodeAtPath(doc.Item1.AsIDocPath(Config), cancellationToken);
        var changes = node.FetchChanges(cancellationToken);

        return View("PollForChangesButton", path);
    }

    public static async Task<bool> ShouldShowPollForChanges(IReadOnlyNode node, IServiceProvider services)
    {
        return await node.CanSync && (await node.Meta).SyncEnabled;
    }

    private async Task<ActionResult> TryActionOnDocument(string path, Func<INodePath, IReadOnlyDocument, Task> fn, CancellationToken cancellationToken, Func<Task<ActionResult>> onValid = null, Func<string, Task<ActionResult>> onInvalid = null)
    {
        var doc = await this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;

        if (Request.Method == "GET")
        {            
            return View(new MoveViewModel { path = doc.Item1, CancelUrl = Request.Headers.Referer });
        }
        else if (Request.Method == "POST")
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await fn(doc.Item1.AsIDocPath(Config), (IReadOnlyDocument)doc.Item2);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                    ModelState.TryAddModelException(nameof(path), e);
                }
            }

            if (ModelState.IsValid)
            {
                return onValid !=  null ? await onValid() : RedirectToAction(nameof(HomeController.Details), "Home", new { path = doc.Item1 });
            }
            else
            {
                return onInvalid !=  null ? await onInvalid(doc.Item1) : View(new MoveViewModel { path = doc.Item1, CancelUrl = Request.Headers.Referer });
            }
        }
        else
        {
            return BadRequest();
        }
    }
}