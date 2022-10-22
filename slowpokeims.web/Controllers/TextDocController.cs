using System;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Controllers;
using SlowPokeIMS.Web.Controllers.Attributes;
using SlowPokeIMS.Web.ViewModels.TextDoc;

namespace SlowPokeIMS.Web.Controllers;

[DocController("text", "text/*", "text/markdown")]
public class TextDocController: Controller
{
    public IDocumentProviderResolver DocumentProviderResolver { get; }
    public Config Config { get; }

    public TextDocController(IDocumentProviderResolver documentProviderResolver, Config config)
    {
        DocumentProviderResolver = documentProviderResolver;
        Config = config;
    }

    [DocAction, HttpGet("text/edit/{path}")]
    public ActionResult Edit(string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return NotFound();
        }
        
        path = Uri.UnescapeDataString(path);
        var node = DocumentProviderResolver.UnifiedReadable.CanRead.GetNodeAtPath(path.AsIDocPath(Config), cancellationToken);
        if (node == null || !node.Exists || !(node is IReadOnlyDocument doc))
        {
            return NotFound();
        }
        
        return View(new NewTextNoteViewModel { path = path, Value = doc.ReadAllText(Encoding.Unicode) });
    }

    [HttpPost("text/edit/{path}")]
    public ActionResult Edit(string path, string Value, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return NotFound();
        }
        
        path = Uri.UnescapeDataString(path);

        var doc = DocumentProviderResolver.UnifiedWritable.CanWrite.GetNodeAtPath(path.AsIDocPath(Config), cancellationToken);
        if (doc == null || !doc.Exists)
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
            writableDoc.WriteIfChanged(s => {
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
            return RedirectToAction(nameof(HomeController.Details), "Home", new { path });
        }
        else
        {
            return View(new NewTextNoteViewModel { path = path, Value = Value, ContentType = DocumentProviderResolver.UnifiedReadable.CanRead.GetContentTypeFromExtension(path.GetFullExtension()) });
        }
    }

    [HttpGet("text/new")]
    public ActionResult NewTextNote() => View(new NewTextNoteViewModel());

    [HttpPost("text/new")]
    public ActionResult NewTextNote(string Value, string ContentType, CancellationToken cancellationToken)
    {
        try
        {
            var rw = DocumentProviderResolver.UnifiedWritable.CanWrite;
            var doc = rw.NewDocument(new NewDocumentOptions { contentType = ContentType }, cancellationToken);

            doc.WriteIfChanged(stream => stream.Write(System.Text.Encoding.UTF8.GetBytes(Value)), cancellationToken);

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