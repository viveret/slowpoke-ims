using System.IO;
using System.Threading.Tasks;
using slowpoke.core.Extensions;
using Xunit;

namespace SlowPokeIMS.Core.Tests.Extensions;


public class StreamExtensionsTests
{
    [Theory]
    [InlineData("old")]
    [InlineData("new")]
    [InlineData("hello world")]
    [InlineData("abcdefghijklmnopqrstuvwxyz")]
    public void ComputeMD5FromStream_Works(string str)
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(str);
        using var ms = new MemoryStream(bytes);
        var hash = ms.ComputeMD5FromStream(true);
        var hashAgain = ms.ComputeMD5FromStream(true);
        
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotNull(hashAgain);
        Assert.NotEmpty(hashAgain);
        Assert.Equal(hash, hashAgain);
    }

    [Theory]
    [InlineData("old", "new")]
    [InlineData("new", "old")]
    [InlineData("hello world", "")]
    [InlineData(" ", "  ")]
    [InlineData("foo bar", "bar foo")]
    [InlineData("0", "1")]
    public void ComputeMD5FromStream_NotEqual(string strA, string strB)
    {
        var bytesA = System.Text.Encoding.ASCII.GetBytes(strA);
        using var msA = new MemoryStream(bytesA);
        var hashA = msA.ComputeMD5FromStream(true);

        var bytesB = System.Text.Encoding.ASCII.GetBytes(strB);
        using var msB = new MemoryStream(bytesB);
        var hashB = msB.ComputeMD5FromStream(true);
        
        Assert.NotNull(hashA);
        Assert.NotEmpty(hashA);
        Assert.NotNull(hashB);
        Assert.NotEmpty(hashB);
        Assert.NotEqual(hashA, hashB);
    }

    [Fact]
    public void ComputeMD5FromStream_Null_Works()
    {
        var hash = Stream.Null.ComputeMD5FromStream(true);
        var hashAgain = Stream.Null.ComputeMD5FromStream(true);
        
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotNull(hashAgain);
        Assert.NotEmpty(hashAgain);
        Assert.Equal(hash, hashAgain);
    }
}