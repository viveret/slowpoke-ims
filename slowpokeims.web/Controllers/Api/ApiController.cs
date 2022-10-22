using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Config;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Web.Controllers.Api;


[ApiController]
public partial class ApiController: ControllerBase
{
    public ApiController(
        Config config,
        IDocumentProviderResolver documentResolver,
        IBroadcastProviderResolver broadcastProviderResolver)
    {
        Config = config;
        DocumentResolver = documentResolver;
        BroadcastProviderResolver = broadcastProviderResolver;
    }

    public IDocumentProviderResolver DocumentResolver { get; }
    public IBroadcastProviderResolver BroadcastProviderResolver { get; }
    public Config Config { get; }




    [HttpGet("api/ping"), HttpHead("api/ping")]
    public ActionResult Ping() => Ok("true");

    [HttpGet("api/can-sync")]
    public ActionResult CanSync() => new JsonResult(this.DocumentResolver.ReadLocal.CanSync);
    
    [HttpGet("api/permissions")]
    public ActionResult Permissions() => new JsonResult(this.DocumentResolver.ReadLocal.Permissions);
    
    [HttpGet("api/resolver-type-name")]
    public ActionResult ResolverTypeName() => new JsonResult(this.DocumentResolver.ReadLocal.ResolverTypeName);
}