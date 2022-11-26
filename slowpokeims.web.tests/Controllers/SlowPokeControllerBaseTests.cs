using Xunit;

using slowpoke.core.Models.Configuration;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Controllers;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace SlowPokeIMS.Web.Tests.Controllers;

public class SlowPokeControllerBaseTests
{
    [Fact]
    public void SlowPokeControllerBaseConstructorDefaultWorks()
    {
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new SlowPokeControllerBase(resolver, cfg);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("!", false)]
    [InlineData("@", false)]
    [InlineData("#", false)]
    [InlineData("$", false)]
    [InlineData("%", false)]
    [InlineData("^", false)]
    [InlineData("&", false)]
    [InlineData("*", false)]
    [InlineData("(", false)]
    [InlineData(")", false)]
    [InlineData("=", false)]
    [InlineData("+", false)]
    [InlineData(".", false)]
    [InlineData("..", false)]
    [InlineData("...", false)]
    [InlineData("<", false)]
    [InlineData(">", false)]
    [InlineData(",", false)]
    [InlineData("?", false)]
    [InlineData(";", false)]
    [InlineData(":", false)]
    [InlineData("'", false)]
    [InlineData("\"", false)]
    [InlineData("[", false)]
    [InlineData("]", false)]
    [InlineData("{", false)]
    [InlineData("}", false)]
    [InlineData("|", false)]
    [InlineData("\\", false)]
    [InlineData("`", false)]
    [InlineData("~", false)]
    [InlineData("/", true)]
    [InlineData("~/", true)]
    public async Task ValidateAndResolvePathWorksAsExpected(string? path, bool isValid)
    {
        path = path != null ? Uri.EscapeDataString(path) : null; // comes in as escaped path
        var cfg = new Config();
        var resolver = new StubDocumentProviderResolver(cfg);
        var controller = new SlowPokeControllerBase(resolver, cfg);

        var result = await controller.ValidateAndResolvePath(path, CancellationToken.None);

        // do not short circuit result if path is valid
        if (isValid)
        {
            Assert.NotNull(result.Item1);
            Assert.NotNull(result.Item2);
            Assert.Null(result.Item3);
        }
        else
        {
            Assert.Null(result.Item2);
            Assert.NotNull(result.Item3);
            // Assert.IsType<NotFoundResult>(result.Item3);
        }
    }
}