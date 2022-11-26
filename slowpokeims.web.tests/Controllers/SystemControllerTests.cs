using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System;
using Xunit;

using slowpoke.core.Models.Configuration;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Util;
using SlowPokeIMS.Web.Controllers;
using slowpoke.core.Services.Scheduled;
using slowpoke.core.Services.Identity;
using SlowPokeIMS.Core.Services.Node;

namespace SlowPokeIMS.Web.Tests.Controllers;

public class SystemControllerTests
{
    [Fact]
    public void SystemControllerConstructorDefault()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new SystemController(cfg, resolver, ScheduledTaskManager.Empty(), new StubIdentityAuthenticationService(), new StubSlowPokeHostProvider());
    }
/*
    [Fact]
    public void HomeControllerSiteIndex()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new HomeController(resolver, new UserSpecialFoldersProvider(resolver, cfg), cfg);
        var result = controller.SiteIndex();

        // SiteIndex();
        // Index();
        // TextNotes();
        // AudioNotes();
        // ImageNotes();
        // Details();
        // Raw();
    }

    [Fact]
    public void HomeControllerDetailsReturns404NullPath()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new HomeController(resolver, new UserSpecialFoldersProvider(resolver, cfg), cfg);
        var result = controller.Details(null, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void HomeControllerDetailsReturns404EmptyPath()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new HomeController(resolver, new UserSpecialFoldersProvider(resolver, cfg), cfg);
        var result = controller.Details(string.Empty, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void HomeControllerDetailsReturns404WhitespacePath()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new HomeController(resolver, new UserSpecialFoldersProvider(resolver, cfg), cfg);
        var result = controller.Details(TestConstants.Whitespace, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void HomeControllerDetailsReturnsSuccessRootPath()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new HomeController(resolver, new UserSpecialFoldersProvider(resolver, cfg), cfg);
        var result = controller.Details(Uri.EscapeDataString(TestConstants.RootPath), CancellationToken.None);

        Assert.IsType<ViewResult>(result);
    }
/*
    [Fact]
    public void HomeControllerConstructorDefault()
    {
        var resolver = new StubDocumentProviderResolver();
        var cfg = new Config();
        var controller = new HomeController(resolver, new UserSpecialFoldersProvider(resolver, cfg), cfg);
    }

    [Fact]
    public void HomeControllerConstructorDefault()
    {
        var resolver = new StubDocumentProviderResolver();
        var cfg = new Config();
        var controller = new HomeController(resolver, new UserSpecialFoldersProvider(resolver, cfg), cfg);
    }

    [Fact]
    public void HomeControllerConstructorDefault()
    {
        var resolver = new StubDocumentProviderResolver();
        var cfg = new Config();
        var controller = new HomeController(resolver, new UserSpecialFoldersProvider(resolver, cfg), cfg);
    }
    */
}