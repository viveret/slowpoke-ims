using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Identity;
using slowpoke.core.Models.SyncState;
using slowpoke.core.Services;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Identity;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using slowpoke.core.Services.Scheduled;
using slowpoke.core.Services.Scheduled.Tasks;

namespace SlowPokeIMS.Web.Controllers.Api;


[ApiController]
public partial class ApiController: ControllerBase
{
    public ApiController(
        Config config,
        IDocumentProviderResolver documentResolver,
        IBroadcastProviderResolver broadcastProviderResolver,
        IScheduledTaskManager scheduledTaskManager,
        ISyncStateManager syncStateManager,
        IIdentityAuthenticationService identityAuthenticationService,
        ISlowPokeHostProvider slowPokeHostProvider)
    {
        Config = config;
        DocumentResolver = documentResolver;
        BroadcastProviderResolver = broadcastProviderResolver;
        this.scheduledTaskManager = scheduledTaskManager;
        SyncStateManager = syncStateManager;
        IdentityAuthenticationService = identityAuthenticationService;
        SlowPokeHostProvider = slowPokeHostProvider;
    }

    public IDocumentProviderResolver DocumentResolver { get; }
    public IBroadcastProviderResolver BroadcastProviderResolver { get; }
    private IScheduledTaskManager scheduledTaskManager;
    public ISyncStateManager SyncStateManager { get; }
    public IIdentityAuthenticationService IdentityAuthenticationService { get; }
    public ISlowPokeHostProvider SlowPokeHostProvider { get; }
    public Config Config { get; }




    // this works
    [HttpGet("api/ping"), HttpHead("api/ping")]
    public ActionResult Ping() => Ok("true");

    [HttpGet("api/can-sync")]
    public async Task<ActionResult> CanSync() => new JsonResult(await (await this.DocumentResolver.ReadLocal).CanSync);

    // this does not work
    [HttpGet("api/sync")]
    public async Task<ActionResult> Sync(bool asynchronous = true, bool immediately = false)
    {
        var taskPull = await scheduledTaskManager.Execute(scheduledTaskManager.GetScheduledTask(typeof(ScanLocalAndPullChangesScheduledTask).FullName), asynchronous: asynchronous, immediately: immediately);
        if (taskPull.Error != null)
        {
            return new JsonResult(taskPull.Error.ToString());
        }
        
        var taskPush = await scheduledTaskManager.Execute(scheduledTaskManager.GetScheduledTask(typeof(ScanLocalAndPublishChangesScheduledTask).FullName), asynchronous: asynchronous, immediately: immediately);
        if (taskPush.Error != null)
        {
            return new JsonResult(taskPush.Error.ToString());
        }

        return new JsonResult(true);
    }

    [HttpGet("api/sync-state")]
    public ActionResult GetSyncState() => new JsonResult(SyncStateManager.GetForSystem().State);
    
    [HttpGet("api/permissions")]
    public async Task<ActionResult> Permissions() => new JsonResult((await this.DocumentResolver.ReadLocal).Permissions);
    
    [HttpGet("api/resolver-type-name")]
    public async Task<ActionResult> ResolverTypeName() => new JsonResult((await this.DocumentResolver.ReadLocal).ResolverTypeName);

    
    [HttpGet("api/add-trusted-test-connection/{hostUrl}")]
    public Task<ActionResult> AddTrustedTestConnection(string hostUrl) => AddTrusted(hostUrl, true);
    
    [HttpGet("api/add-trusted/{hostUrl}")]
    public async Task<ActionResult> AddTrusted(string hostUrl, bool testConnection = false)
    {
        if (string.IsNullOrWhiteSpace(hostUrl))
        {
            return BadRequest();
        }
        
        hostUrl = Uri.UnescapeDataString(hostUrl);

        var hostUri = new Uri(hostUrl);
        var identity = IdentityAuthenticationService.GetIdentityFromEndpoint(hostUri, CancellationToken.None);

        if (identity != null)
        {
            IdentityAuthenticationService.SetTrustLevel(identity, TrustLevel.Trusted, CancellationToken.None);
            var persistedValue = IdentityAuthenticationService.GetTrustLevel(identity, CancellationToken.None);
            if (persistedValue == TrustLevel.Trusted)
            {
                if (testConnection)
                {
                    var client = await SlowPokeHostProvider.OpenClient(hostUri, cancellationToken: CancellationToken.None);
                    if (!await client.Ping(CancellationToken.None))
                    {
                        return new JsonResult("Could not ping or connect");
                    }
                }
                return new JsonResult(true);
            }
            else
            {
                return new JsonResult($"Invalid persisted trust level {persistedValue}");
            }
        }
        else
        {
            return new JsonResult($"identity {hostUrl} not found");
        }
    }
    
    [HttpGet("api/list-trusted")]
    public async Task<ActionResult> ListTrusted()
    {
        return new JsonResult(SlowPokeHostProvider.Trusted.ToArray());
    }
}