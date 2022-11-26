using Microsoft.AspNetCore.Mvc;
using Xunit;

using slowpoke.core.Models.Configuration;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Controllers;
using SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Identity;
using slowpoke.core.Services.Http;
using System.Collections.Generic;
using SlowPokeIMS.Core.Services.Broadcast;
using System.Threading.Tasks;

namespace SlowPokeIMS.Web.Tests.Controllers;

public class TestControllerTests
{
    [Fact]
    public void TestController_ConstructorDefault()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new TestController(resolver, cfg); // ScheduledTaskManager.Empty(), new StubIdentityAuthenticationService(), new StubSlowPokeHostProvider()
    }

    [Fact]
    public void TestController_Index_Works()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new TestController(resolver, cfg);
        Assert.IsType<ViewResult>(controller.Index());
    }

    [Fact]
    public async Task TestController_CreateFolder_Works()
    {
        var controller = await CreateController();
        var result = await controller.CreateFolder("~/awd/", true.ToString());
        Assert.IsType<OkObjectResult>(result);
        var obj = ((OkObjectResult)result).Value?.ToString();
        Assert.Equal("true", obj);
    }

    private static Task<TestController> CreateController()
    {
        var cfg = new Config();
        var hostProvider = new LocalSlowPokeHostProvider(cfg, new LocalIdentityAuthenticationService(cfg), new IpAddressHistoryService());
        var broadcastProviderResolver = BroadcastProviderResolver.CreateForTesting(cfg, hostProvider);
        var repo = new Core.Services.Node.Docs.InMemoryGenericDocumentRepository();
        var resolver = new GenericDocumentProviderResolver(cfg, broadcastProviderResolver)
        {
            ReadLocal = Task.FromResult<IReadOnlyDocumentResolver>(new GenericReadOnlyDocumentResolver(cfg, broadcastProviderResolver, repo)),
            ReadWriteLocal = Task.FromResult<IWritableDocumentResolver>(new GenericReadWriteDocumentResolver(cfg, repo, broadcastProviderResolver)),
        };
        var controller = new TestController(resolver, cfg);
        return Task.FromResult(controller);
    }

    [Fact]
    public async Task TestController_CreateFile_Works()
    {
        var controller = await CreateController();
        var result = await controller.CreateFile("~/awd.txt", true.ToString());
        Assert.IsType<OkObjectResult>(result);
        var obj = ((OkObjectResult) result).Value?.ToString();
        Assert.Equal("true", obj);
    }

    [Fact]
    public async Task TestController_EnsureNoFilesOrFolders_Works()
    {
        var controller = await CreateController();
        var result = await controller.EnsureNoFilesOrFolders();
        Assert.IsType<OkObjectResult>(result);
        var obj = ((OkObjectResult) result).Value?.ToString();
        Assert.Equal("true", obj);
    }
}