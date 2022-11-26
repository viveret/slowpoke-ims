using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System;
using Xunit;

using slowpoke.core.Models.Configuration;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Util;
using SlowPokeIMS.Web.Controllers;
using System.Threading.Tasks;

namespace SlowPokeIMS.Web.Tests.Controllers;

public class HomeControllerTests
{
    [Fact]
    public void HomeControllerConstructorDefault()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new HomeController(resolver, cfg);
    }

    [Fact]
    public void HomeControllerSiteIndex()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new HomeController(resolver, cfg);
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
    public async Task HomeControllerDetailsReturns404NullPath()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new HomeController(resolver, cfg);
        var result = await controller.Details(null, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task HomeControllerDetailsReturns404EmptyPath()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new HomeController(resolver, cfg);
        var result = await controller.Details(string.Empty, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task HomeControllerDetailsReturns404WhitespacePath()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new HomeController(resolver, cfg);
        var result = await controller.Details(TestConstants.Whitespace, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task HomeControllerDetailsReturnsSuccessRootPath()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new HomeController(resolver, cfg);
        var result = await controller.Details(Uri.EscapeDataString(TestConstants.RootPath), CancellationToken.None);

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