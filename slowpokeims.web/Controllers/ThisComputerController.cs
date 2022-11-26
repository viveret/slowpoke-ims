using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Controllers.Attributes;
using SlowPokeIMS.Web.ViewModels.ThisComputer;

namespace SlowPokeIMS.Web.Controllers;


[ShowInNavBar("This Computer", showInHorizontal: false)]
public class ThisComputerController : DefaultRoutesBrowseDocsController<IndexViewModel, DetailsViewModel>
{
    public ThisComputerController(
        IDocumentProviderResolver documentResolver, 
        //IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver/*, userSpecialFoldersProvider*/, config)
    {
    }
    
    [ShowInNavBar("Everything", showInHorizontal: false), HttpGet("this-computer/search")]
    public Task<ActionResult> Index([FromQuery] QueryDocumentOptions q, CancellationToken cancellationToken) => base.Index(q, cancellationToken);

    [ShowInNavBar("Text Notes", showInHorizontal: false), HttpGet("this-computer/text-notes")]
    public Task<ActionResult> TextNotes([FromQuery] QueryDocumentOptions q, CancellationToken cancellationToken) => base.TextNotes(q, cancellationToken);
    
    [ShowInNavBar("Audio Notes", showInHorizontal: false), HttpGet("this-computer/audio-notes")]
    public Task<ActionResult> AudioNotes([FromQuery] QueryDocumentOptions q, CancellationToken cancellationToken)=> base.AudioNotes(q, cancellationToken);
    
    [ShowInNavBar("Image Notes", showInHorizontal: false), HttpGet("this-computer/image-notes")]
    public Task<ActionResult> ImageNotes([FromQuery] QueryDocumentOptions q, CancellationToken cancellationToken) => base.ImageNotes(q, cancellationToken);

    [HttpGet("this-computer/details/{path}")]
    public Task<ActionResult> Details(string path, CancellationToken cancellationToken) => base.Details(path, cancellationToken);

    protected override void ForceControllerSpecificOptions(QueryDocumentOptions q)
    {
        q.Path ??= "/".AsIDocPath(Config);
        q.Recursive = false;
        q.IncludeFolders = true;
    }
}