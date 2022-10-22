using System.Threading;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Config;
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
        IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver, userSpecialFoldersProvider, config)
    {
    }

    [HttpGet("")]
    public ActionResult SiteIndex() => RedirectToAction(nameof(Index));
    
    [ShowInNavBar("Everything", showInHorizontal: false), HttpGet("home/search")]
    public ActionResult Index([Bind(Prefix = "r.q")] QueryDocumentOptions q, CancellationToken cancellationToken) => base.Index(q, cancellationToken);

    [ShowInNavBar("Text Notes", showInHorizontal: false), HttpGet("home/text-notes")]
    public ActionResult TextNotes([Bind(Prefix = "r.q")] QueryDocumentOptions q, CancellationToken cancellationToken) => base.TextNotes(q, cancellationToken);
    
    [ShowInNavBar("Audio Notes", showInHorizontal: false), HttpGet("home/sound-notes")]
    public ActionResult AudioNotes([Bind(Prefix = "r.q")] QueryDocumentOptions q, CancellationToken cancellationToken) => base.AudioNotes(q, cancellationToken);
    
    [ShowInNavBar("Image Notes", showInHorizontal: false), HttpGet("home/image-notes")]
    public ActionResult ImageNotes([Bind(Prefix = "r.q")] QueryDocumentOptions q, CancellationToken cancellationToken) => base.ImageNotes(q, cancellationToken);

    [HttpGet("home/details/{path}")]
    public ActionResult Details(string path, CancellationToken cancellationToken) => base.Details(path, cancellationToken);

    [HttpGet("home/raw/{path}")]
    public ActionResult Raw(string path, CancellationToken cancellationToken) => base.Raw(path, cancellationToken);


    protected override void ForceControllerSpecificOptions(QueryDocumentOptions q)
    {
        q.Path ??= Config.Paths.HomePath.AsIDocPath(Config);
        q.Recursive = true;
    }
}