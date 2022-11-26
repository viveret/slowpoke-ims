using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Controllers.Attributes;
using SlowPokeIMS.Web.ViewModels.TextDoc;

namespace SlowPokeIMS.Web.Controllers;

[DocController("text", "text/*", "text/markdown")]
public class TextDocController: SlowPokeControllerBase
{
    public TextDocController(IDocumentProviderResolver documentProviderResolver, Config config): base(documentProviderResolver, config)
    {
    }

    [DocAction, HttpGet("text/edit/{path}")]
    public async Task<ActionResult> Edit(string path, CancellationToken cancellationToken)
    {
        var pathResult = await ValidateAndResolvePath(path, cancellationToken);
        if (pathResult.Item3 != null)
        {
            return pathResult.Item3;
        }
        
        var node = await (await DocumentResolver.UnifiedReadable).CanRead.GetNodeAtPath(pathResult.Item1.AsIDocPath(Config), cancellationToken);
        if (node == null || !await node.Exists || !(node is IReadOnlyDocument doc))
        {
            return NotFound();
        }
        
        return View(new NewTextNoteViewModel { path = pathResult.Item1, Value = await doc.ReadAllText(Encoding.Unicode) });
    }

    [HttpPost("text/edit/{path}")]
    public async Task<ActionResult> Edit(string path, string Value, CancellationToken cancellationToken)
    {
        var pathResult = await ValidateAndResolvePath(path, cancellationToken);
        if (pathResult.Item3 != null)
        {
            return pathResult.Item3;
        }

        var doc = await (await DocumentResolver.UnifiedWritable).CanWrite.GetNodeAtPath(pathResult.Item1.AsIDocPath(Config), cancellationToken);
        if (doc == null || !await doc.Exists)
        {
            return NotFound();
        }

        var writableDoc = doc as IWritableDocument;
        if (writableDoc == null)
        {
            return Unauthorized();
        }

        try
        {
            if (Value != null && Value.Length > 1000000000)
            {
                throw new ArgumentOutOfRangeException(nameof(Value));
            }

            // file is being used by another process
            await writableDoc.WriteIfChanged(s => {
                s.Write(System.Text.Encoding.Unicode.GetBytes(Value));
            }, cancellationToken);
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine(e);
            ModelState.TryAddModelException(nameof(Value), e);
        }

        if (ModelState.IsValid)
        {
            return RedirectToAction(nameof(HomeController.Details), "Home", new { path = pathResult.Item1 });
        }
        else
        {
            return View(new NewTextNoteViewModel { path = pathResult.Item1, Value = Value, ContentType = await (await DocumentResolver.UnifiedReadable).CanRead.GetContentTypeFromExtension(pathResult.Item1.GetFullExtension()) });
        }
    }

    [HttpGet("text/new")]
    public ActionResult NewTextNote() => View(new NewTextNoteViewModel());

    [HttpPost("text/new")]
    public async Task<ActionResult> NewTextNote(string Value, string ContentType, CancellationToken cancellationToken)
    {
        try
        {
            var rw = (await DocumentResolver.UnifiedWritable).CanWrite;
            var doc = await rw.NewDocument(new NewDocumentOptions { contentType = ContentType }, cancellationToken);

            await doc.WriteIfChanged(stream => stream.Write(System.Text.Encoding.UTF8.GetBytes(Value)), cancellationToken);

            return RedirectToAction(nameof(HomeController.Index), "Home", new { path = doc.Path.PathValue });
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.Write(e);
            ModelState.AddModelError(nameof(Value), $"{e.Message}\n{e.StackTrace}");
        }

        // maybe return result and check status? otherwise maybe rely on thrown exceptions
        return View(new NewTextNoteViewModel { Value = Value, ContentType = ContentType });
    }
}