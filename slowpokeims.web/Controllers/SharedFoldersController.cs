using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Controllers.Attributes;
using SlowPokeIMS.Web.ViewModels.SharedFolders;

namespace SlowPokeIMS.Web.Controllers;


[ShowInNavBar("Shared Folders", tooltip: "Folders shared across networks", showInHorizontal: false)]
public class SharedFoldersController : DefaultRoutesBrowseDocsController<IndexViewModel, DetailsViewModel>
{
    public SharedFoldersController(
        IDocumentProviderResolver documentResolver,
        //IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver/*, userSpecialFoldersProvider*/, config)
    {
    }
    
    [ShowInNavBar("Everything", showInHorizontal: false), HttpGet("shared-folders/search")]
    public Task<ActionResult> Index([Bind] QueryDocumentOptions q, CancellationToken cancellationToken) => base.Index(q, cancellationToken);
    
    [ShowInNavBar("Text Notes", showInHorizontal: false), HttpGet("shared-folders/text-notes")]
    public Task<ActionResult> TextNotes([Bind] QueryDocumentOptions q, CancellationToken cancellationToken) => base.TextNotes(q, cancellationToken);
    
    [ShowInNavBar("Audio Notes", showInHorizontal: false), HttpGet("shared-folders/sound-notes")]
    public Task<ActionResult> AudioNotes([Bind] QueryDocumentOptions q, CancellationToken cancellationToken) => base.AudioNotes(q, cancellationToken);
    
    [ShowInNavBar("Image Notes", showInHorizontal: false), HttpGet("shared-folders/image-notes")]
    public Task<ActionResult> ImageNotes([Bind] QueryDocumentOptions q, CancellationToken cancellationToken) => base.ImageNotes(q, cancellationToken);

    [HttpGet("shared-folders/details/{path}")]
    public Task<ActionResult> Details(string path, CancellationToken cancellationToken) => base.Details(path, cancellationToken);

    protected override void ForceControllerSpecificOptions(QueryDocumentOptions q)
    {
        q.Path ??= "/".AsIDocPath(Config);
        q.IncludeFolders = true;
        q.Recursive = false;
    }
}