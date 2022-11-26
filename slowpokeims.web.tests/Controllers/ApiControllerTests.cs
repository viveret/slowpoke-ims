using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System;
using Xunit;

using slowpoke.core.Models.Configuration;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Util;
using SlowPokeIMS.Web.Controllers;
using SlowPokeIMS.Web.Controllers.Api;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Scheduled;
using System.Linq;
using slowpoke.core.Services;
using System.Threading.Tasks;
using slowpoke.core.Services.Scheduled.Tasks;
using System.Collections.Generic;
using slowpoke.core.Models.SyncState;
using slowpoke.core.Services.Identity;
using SlowPokeIMS.Tests.Core.Services;
using slowpoke.core.Models.Identity;
using SlowPokeIMS.Core.Services.Node;
using SlowPokeIMS.Integration.Tests.Core;
using slowpoke.core.Models.Node;

namespace SlowPokeIMS.Web.Tests.Controllers;

public class ApiControllerTests: IClassFixture<WebServerFixture>
{
    private readonly WebServerFixture fixture;

    public ApiControllerTests(WebServerFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ApiControllerConstructorDefault()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var broadcastProviderResolver = new StubBroadcastProviderResolver();
        var scheduledTaskManager = new ScheduledTaskManager(new ScheduledTaskContextStorage(), Enumerable.Empty<IScheduledTask>());
        var authService = new TestIdentityAuthenticationService(cfg);
        var hostService = new StubSlowPokeHostProvider();
        var controller = new ApiController(cfg, resolver, broadcastProviderResolver, scheduledTaskManager, new SyncStateManager(), authService, hostService);
    }
    
    [Fact]
    public void ApiControllerPingWorks()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var broadcastProviderResolver = new StubBroadcastProviderResolver();
        var scheduledTaskManager = new ScheduledTaskManager(new ScheduledTaskContextStorage(), Enumerable.Empty<IScheduledTask>());
        var authService = new TestIdentityAuthenticationService(cfg);
        var hostService = new StubSlowPokeHostProvider();
        var controller = new ApiController(cfg, resolver, broadcastProviderResolver, scheduledTaskManager, new SyncStateManager(), authService, hostService);
        var result = controller.Ping();

        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }
    
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task ApiController_Sync_Start(bool asynchronous, bool immediately)
    {
        // todo: implement test that checks if the task completes successfully for empty (default scenario)
        // otherwise this test does not really test sync
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var broadcastProviderResolver = new StubBroadcastProviderResolver();
        var syncMgr = new SyncStateManager();
        syncMgr.ClearStaticVars();
        var tasks = new List<IScheduledTask>()
        {
            new ScanLocalAndPullChangesScheduledTask(cfg, syncMgr, null, resolver),
            new ScanLocalAndPublishChangesScheduledTask(cfg, syncMgr, null, resolver, broadcastProviderResolver)
        };
        var scheduledTaskManager = new ScheduledTaskManager(new ScheduledTaskContextStorage(), tasks);
        var authService = new TestIdentityAuthenticationService(cfg);
        var hostService = new TestSlowPokeHostProvider(cfg, fixture, authService);
        var controller = new ApiController(cfg, resolver, broadcastProviderResolver, scheduledTaskManager, new SyncStateManager(), authService, hostService);
        var result = await controller.Sync(asynchronous: asynchronous, immediately: immediately);

        Assert.NotNull(result);
        Assert.IsType<JsonResult>(result);
        /*
        Assert.Equal(true, ((JsonResult)result).Value);

        var systemState = syncMgr.GetForSystem();
        if (asynchronous)
        {
            if (immediately)
            {
                Assert.Equal(SyncState.Uninitialized, systemState.State);
            }
            else
            {
                Assert.Equal(SyncState.Uninitialized, systemState.State);
            }
        }
        else
        {
            Assert.Equal(SyncState.UpToDate, systemState.State);
        }*/
    }

    // todo: write tests that use in-memory document providers w/ folders & hierarchy to simulate home folder
    // or to simulate root folder, or different disks/drives/devices
    
    [Fact]
    public async Task ApiController_AddTrusted_Works()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var broadcastProviderResolver = new StubBroadcastProviderResolver();
        var scheduledTaskManager = new ScheduledTaskManager(new ScheduledTaskContextStorage(), Enumerable.Empty<IScheduledTask>());

        var authService = new TestIdentityAuthenticationService(cfg);
        authService.Identities[Guid.Empty] = new IdentityModel();
        authService.IdentitiesByEndpoint[new Uri("https://automatedtest.local.client/2")] = new IdentityModel();

        var hostService = new TestSlowPokeHostProvider(cfg, fixture, authService);
        var controller = new ApiController(cfg, resolver, broadcastProviderResolver, scheduledTaskManager, new SyncStateManager(), authService, hostService);
        var result = await controller.AddTrusted("https://automatedtest.local.client/2", testConnection: true);

        Assert.NotNull(result);
        Assert.IsType<JsonResult>(result);

        var jsonResult = (JsonResult) result;
        Assert.Equal(true, jsonResult.Value);

        result = await controller.ListTrusted();
        Assert.NotNull(result);
        Assert.IsType<JsonResult>(result);
        jsonResult = (JsonResult) result;
        Assert.IsType<ISlowPokeHost[]>(jsonResult.Value);

        var trustedList = (ISlowPokeHost[])jsonResult.Value!;
        Assert.NotEmpty(trustedList);
    }
}