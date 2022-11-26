using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Controllers.Attributes;
using SlowPokeIMS.Web.ViewModels.Home;

namespace SlowPokeIMS.Web.Controllers;


[ShowInNavBar("Home", showInHorizontal: false)]
public class HomeController : DefaultRoutesBrowseDocsController<IndexViewModel, DetailsViewModel>
{
    public HomeController(
        IDocumentProviderResolver documentResolver,
        //IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver/*, userSpecialFoldersProvider*/, config)
    {
    }

    [HttpGet("")]
    public ActionResult SiteIndex() => RedirectToAction(nameof(Index));
    
    [ShowInNavBar("Everything", showInHorizontal: false), HttpGet("home/search")]
    public Task<ActionResult> Index([Bind(Prefix = "r.q")] QueryDocumentOptions q, CancellationToken cancellationToken) => base.Index(q, cancellationToken);

    [ShowInNavBar("Text Notes", showInHorizontal: false), HttpGet("home/text-notes")]
    public Task<ActionResult> TextNotes([Bind(Prefix = "r.q")] QueryDocumentOptions q, CancellationToken cancellationToken) => base.TextNotes(q, cancellationToken);
    
    [ShowInNavBar("Audio Notes", showInHorizontal: false), HttpGet("home/sound-notes")]
    public Task<ActionResult> AudioNotes([Bind(Prefix = "r.q")] QueryDocumentOptions q, CancellationToken cancellationToken) => base.AudioNotes(q, cancellationToken);
    
    [ShowInNavBar("Image Notes", showInHorizontal: false), HttpGet("home/image-notes")]
    public Task<ActionResult> ImageNotes([Bind(Prefix = "r.q")] QueryDocumentOptions q, CancellationToken cancellationToken) => base.ImageNotes(q, cancellationToken);

    [HttpGet("home/details/{path}")]
    public Task<ActionResult> Details(string path, CancellationToken cancellationToken) => base.Details(path, cancellationToken);

    [HttpGet("home/raw/{path}")]
    public Task<ActionResult> Raw(string path, CancellationToken cancellationToken) => base.Raw(path, cancellationToken);


    protected override void ForceControllerSpecificOptions(QueryDocumentOptions q)
    {
        q.Path ??= Config.Paths.HomePath.AsIDocPath(Config);
        q.Recursive = true;
    }
}