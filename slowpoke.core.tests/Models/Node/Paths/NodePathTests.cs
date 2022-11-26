using System;
using Xunit;

using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Configuration;

namespace SlowPokeIMS.Core.Tests.Models.Node.Paths;

public class NodePathTests
{
    [Fact]
    public void NodePath_Root_Works()
    {
        var config = new Config();
        var path = "/".AsIDocPath(config);
        Assert.IsType<DocPathAbsolute>(path);
        Assert.True(path.IsFolder);
        Assert.True(path.IsAbsolute);
        Assert.False(path.IsRelative);
        Assert.False(path.IsMeta);
        Assert.False(path.IsDocument);
        Assert.False(path.IsUri);
    }

    [Theory]
    [InlineData("/bin/")]
    [InlineData("/home/")]
    [InlineData("/tmp/")]
    public void NodePath_Absolute_Works(string path)
    {
        var config = new Config();
        var docPath = path.AsIDocPath(config);
        Assert.IsType<DocPathAbsolute>(docPath);
        Assert.True(docPath.IsFolder);
        Assert.True(docPath.IsAbsolute);
        Assert.False(docPath.IsRelative);
        Assert.False(docPath.IsMeta);
        Assert.False(docPath.IsDocument);
        Assert.False(docPath.IsUri);
    }

    [Theory]
    [InlineData("~/bin/")]
    [InlineData("~/home/")]
    [InlineData("~/tmp/")]
    public void NodePath_Relative_Works(string path)
    {
        var config = new Config();
        var docPath = path.AsIDocPath(config);
        Assert.IsType<DocPathRelative>(docPath);
        Assert.True(docPath.IsFolder);
        Assert.True(docPath.IsRelative);
        Assert.False(docPath.IsAbsolute);
        Assert.False(docPath.IsMeta);
        Assert.False(docPath.IsDocument);
        Assert.False(docPath.IsUri);
    }

    [Theory]
    [InlineData("https://127.0.0.1/bin/")]
    [InlineData("https://127.0.0.1/home/")]
    [InlineData("https://127.0.0.1/tmp/")]
    public void NodePath_Uri_Works(string path)
    {
        var config = new Config();
        var docPath = path.AsIDocPath(config);
        Assert.IsType<DocPathUri>(docPath);
        Assert.True(docPath.IsFolder);
        Assert.True(docPath.IsUri);
        Assert.False(docPath.IsAbsolute);
        Assert.False(docPath.IsRelative);
        Assert.False(docPath.IsMeta);
        Assert.False(docPath.IsDocument);
    }

    [Fact]
    public void NodePath_Empty_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => {
            var config = new Config();
            "".AsIDocPath(config);
        });
    }

    [Theory]
    [InlineData("~")]
    [InlineData("!")]
    [InlineData("@")]
    [InlineData("#")]
    [InlineData("$")]
    [InlineData("%")]
    [InlineData("^")]
    [InlineData("&")]
    [InlineData("*")]
    [InlineData("=")]
    [InlineData("{")]
    [InlineData("}")]
    [InlineData("[")]
    [InlineData("]")]
    [InlineData("\\")]
    [InlineData(";")]
    [InlineData(":")]
    [InlineData("\"")]
    [InlineData("?")]
    public void NodePath_Invalid_ThrowsException(string path)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => {
            var config = new Config();
            Assert.IsType<INodePath>(path.AsIDocPath(config));
        });
    }
}