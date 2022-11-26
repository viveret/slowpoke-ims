using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Controllers.Attributes;
using SlowPokeIMS.Web.ViewModels.AllDevices;

namespace SlowPokeIMS.Web.Controllers;


[ShowInNavBar("All Devices", tooltip: "Items shared across all devices", showInHorizontal: false)]
public class AllDevicesController : DefaultRoutesBrowseDocsController<IndexViewModel, DetailsViewModel>
{
    public AllDevicesController(
        IDocumentProviderResolver documentResolver,
        //IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver/*, userSpecialFoldersProvider*/, config)
    {
    }
    
    [ShowInNavBar("Everything", showInHorizontal: false), HttpGet("all-devices/search")]
    public Task<ActionResult> Index([Bind] QueryDocumentOptions q, CancellationToken cancellationToken) => base.Index(q, cancellationToken);
    
    [ShowInNavBar("Text Notes", showInHorizontal: false), HttpGet("all-devices/text-notes")]
    public Task<ActionResult> TextNotes([Bind] QueryDocumentOptions q, CancellationToken cancellationToken) => base.TextNotes(q, cancellationToken);
    
    [ShowInNavBar("Audio Notes", showInHorizontal: false), HttpGet("all-devices/sound-notes")]
    public Task<ActionResult> AudioNotes([Bind] QueryDocumentOptions q, CancellationToken cancellationToken) => base.AudioNotes(q, cancellationToken);
    
    [ShowInNavBar("Image Notes", showInHorizontal: false), HttpGet("all-devices/image-notes")]
    public Task<ActionResult> ImageNotes([Bind] QueryDocumentOptions q, CancellationToken cancellationToken) => base.ImageNotes(q, cancellationToken);

    [HttpGet("all-devices/details/{path}")]
    public Task<ActionResult> Details(string path, CancellationToken cancellationToken) => base.Details(path, cancellationToken);

    protected override void ForceControllerSpecificOptions(QueryDocumentOptions q)
    {
        q.Path ??= "/".AsIDocPath(Config);
        q.IncludeFolders = true;
        q.Recursive = false;
    }
}