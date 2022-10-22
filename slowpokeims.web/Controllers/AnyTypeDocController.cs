using System;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Config;
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
    public ActionResult Copy(string path, CancellationToken cancellationToken)
    {
        var doc = ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;
        
        return View(new CopyViewModel { path = doc.Item1, DestinationDefault = GetDestinationDefault(doc.Item1, cancellationToken), CancelUrl = Request.Headers.Referer });
    }

    [HttpPost("doc/copy/{path}")]
    public ActionResult Copy(string path, string Destination, CancellationToken cancellationToken)
    {
        return TryActionOnDocument(path, (INodePath path, IReadOnlyDocument doc) =>
        {
            var destDefault = GetDestinationDefault(path.PathValue, cancellationToken);
            var dest = string.IsNullOrWhiteSpace(Destination) ? destDefault : (Destination = Uri.UnescapeDataString(Destination));
            var destPath = dest.AsIDocPath(Config);
            if (DocumentResolver.UnifiedReadable.CanRead.NodeExistsAtPath(destPath, cancellationToken))
            {
                ModelState.AddModelError(nameof(Destination), "Document already exists at destination");
            }

            if (ModelState.IsValid)
            {
                DocumentResolver.UnifiedWritable.CanWrite.CopyDocumentTo(path, destPath, cancellationToken);
            }
        }, cancellationToken, () => RedirectToAction(nameof(HomeController.Details), "Home", new { path = Destination }),
            (string path) => View(new CopyViewModel { path = path, Destination = Destination, DestinationDefault = GetDestinationDefault(path, cancellationToken), CancelUrl = Request.Headers.Referer }));
    }

    private string GetDestinationDefault(string path, CancellationToken cancellationToken)
    {
        var ext = path.GetFullExtension();
        var newPath = path.Substring(0, path.Length - ext.Length) + "(copy)" + ext;
        var numTries = 1;
        while (DocumentResolver.UnifiedReadable.CanRead.NodeExistsAtPath(newPath.AsIDocPath(Config), cancellationToken))
        {
            numTries++;
            newPath = path.Substring(0, path.Length - ext.Length) + $"(copy {numTries})" + ext;
        }
        return newPath;
    }

    [DocAction, HttpGet("doc/move/{path}")]
    public ActionResult Move(string path, CancellationToken cancellationToken)
    {
        var doc = this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;
        
        return View(new MoveViewModel { path = doc.Item1, CancelUrl = Request.Headers.Referer });
    }

    [HttpPost("doc/move/{path}")]
    public ActionResult Move(string path, string Destination, CancellationToken cancellationToken)
    {
        return TryActionOnDocument(path, (INodePath path, IReadOnlyDocument doc) =>
        {
            Destination = Uri.UnescapeDataString(Destination);
            var destPath = Destination.AsIDocPath(Config);
            if (DocumentResolver.UnifiedReadable.CanRead.NodeExistsAtPath(destPath, cancellationToken))
            {
                ModelState.AddModelError(nameof(Destination), "Document already exists at destination");
            }

            if (ModelState.IsValid)
            {
                DocumentResolver.UnifiedWritable.CanWrite.MoveDocumentTo(path, destPath, cancellationToken);
            }
        }, cancellationToken, () => RedirectToAction(nameof(HomeController.Details), "Home", new { path = Destination }),
            (string path) => View(new MoveViewModel { path = path, Destination = Destination, CancelUrl = Request.Headers.Referer }));
    }

    [DocAction, HttpGet("doc/archive/{path}"), HttpPost("doc/archive/{path}")]
    public ActionResult Archive(string path, CancellationToken cancellationToken)
    {
        return TryActionOnDocument(path, (INodePath path, IReadOnlyDocument doc) => DocumentResolver.UnifiedWritable.CanWrite.ArchiveDocumentAtPath(path, cancellationToken), cancellationToken);
    }

    [DocAction, HttpGet("doc/delete/{path}"), HttpPost("doc/delete/{path}")]
    public ActionResult Delete(string path, CancellationToken cancellationToken)
    {
        return TryActionOnDocument(path, (INodePath path, IReadOnlyDocument doc) => DocumentResolver.UnifiedWritable.CanWrite.DeleteDocumentAtPath(path, cancellationToken), cancellationToken);
    }

    [DocAction(inlineAction: true), DocActionLabel("Turn on Sync"), HttpPost("doc/turn-off-sync/{path}")]
    [DocActionVisibility(typeof(AnyTypeDocController), nameof(ShouldShowTurnOffSync))]
    public ActionResult TurnOffSync(string path, CancellationToken cancellationToken)
    {
        var doc = this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;

        DocumentResolver.UnifiedWritable.CanWrite.GetNodeAtPath(doc.Item1.AsIDocPath(Config), cancellationToken).TurnOffSync(cancellationToken);

        return View("TurnOnSyncButton");
    }

    public static bool ShouldShowTurnOffSync(IReadOnlyNode node, IServiceProvider services)
    {
        return node.CanSync && node.Meta.SyncEnabled;
    }

    [DocAction(inlineAction: true), DocActionLabel("Turn on Sync"), HttpPost("doc/turn-on-sync/{path}")]
    [DocActionVisibility(typeof(AnyTypeDocController), nameof(ShouldShowTurnOnSync))]
    public ActionResult TurnOnSync(string path, CancellationToken cancellationToken)
    {
        var doc = this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;

        DocumentResolver.UnifiedWritable.CanWrite.GetNodeAtPath(doc.Item1.AsIDocPath(Config), cancellationToken).TurnOnSync(cancellationToken);

        return View("TurnOffSyncButton");
    }

    public  static bool ShouldShowTurnOnSync(IReadOnlyNode node, IServiceProvider services)
    {
        return node.CanSync && !node.Meta.SyncEnabled;
    }

    [DocAction(inlineAction: true), DocActionLabel("Broadcast Changes"), HttpPost("doc/broadcast-changes/{path}")]
    [DocActionVisibility(typeof(AnyTypeDocController), nameof(ShouldShowBroadcastChanges))]
    public ActionResult BroadcastChanges(string path, CancellationToken cancellationToken)
    {
        var doc = this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;

        DocumentResolver.UnifiedWritable.CanWrite.GetNodeAtPath(doc.Item1.AsIDocPath(Config), cancellationToken).BroadcastChanges(cancellationToken);

        return View("BroadcastChangesButton", path);
    }

    public static bool ShouldShowBroadcastChanges(IReadOnlyNode node, IServiceProvider services)
    {
        return node.CanSync && node.Meta.SyncEnabled;
    }

    [DocAction(inlineAction: true), DocActionLabel("Poll For Changes"), HttpPost("doc/poll-for-changes/{path}")]
    [DocActionVisibility(typeof(AnyTypeDocController), nameof(ShouldShowPollForChanges))]
    public ActionResult PollForChanges(string path, CancellationToken cancellationToken)
    {
        var doc = this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;

        var changes = DocumentResolver.UnifiedWritable.CanWrite.GetNodeAtPath(doc.Item1.AsIDocPath(Config), cancellationToken).FetchChanges(cancellationToken);

        return View("PollForChangesButton", path);
    }

    public static bool ShouldShowPollForChanges(IReadOnlyNode node, IServiceProvider services)
    {
        return node.CanSync && node.Meta.SyncEnabled;
    }

    private ActionResult TryActionOnDocument(string path, Action<INodePath, IReadOnlyDocument> fn, CancellationToken cancellationToken, Func<ActionResult> onValid = null, Func<string, ActionResult> onInvalid = null)
    {
        var doc = this.ValidateAndResolvePath(path, cancellationToken);
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
                    fn(doc.Item1.AsIDocPath(Config), (IReadOnlyDocument)doc.Item2);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                    ModelState.TryAddModelException(nameof(path), e);
                }
            }

            if (ModelState.IsValid)
            {
                return onValid !=  null ? onValid() : RedirectToAction(nameof(HomeController.Details), "Home", new { path = doc.Item1 });
            }
            else
            {
                return onInvalid !=  null ? onInvalid(doc.Item1) : View(new MoveViewModel { path = doc.Item1, CancelUrl = Request.Headers.Referer });
            }
        }
        else
        {
            return BadRequest();
        }
    }
}