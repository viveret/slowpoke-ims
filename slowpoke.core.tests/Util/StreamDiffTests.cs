using System;
using Xunit;

using slowpoke.core.Util;
using System.IO;
using System.Threading;

namespace SlowPokeIMS.Core.Tests.Util;

public class StreamDiffTests
{
    [Fact]
    public void StreamDiff_Constructor_Default()
    {
        using var streamOld = new MemoryStream();
        using var streamNew = new MemoryStream();
        var diff = new StreamDiff(streamOld, streamNew, true, true, CancellationToken.None);
    }

    [Fact]
    public void StreamDiff_MemoryStream_Empty_NoDifference()
    {
        using var streamOld = new MemoryStream();
        using var streamNew = new MemoryStream();
        var diff = new StreamDiff(streamOld, streamNew, true, true, CancellationToken.None);
        Assert.False(diff.HasChanged);
        Assert.NotNull(diff.OriginalHash);
        Assert.NotNull(diff.NewHash);
        Assert.Equal(diff.OriginalHash, diff.NewHash);
    }

    [Fact]
    public void StreamDiff_NullStream_Empty_NoDifference()
    {
        var diff = new StreamDiff(Stream.Null, Stream.Null, false, true, CancellationToken.None);
        Assert.False(diff.HasChanged);
        Assert.NotNull(diff.OriginalHash);
        Assert.NotNull(diff.NewHash);
        Assert.Equal(diff.OriginalHash, diff.NewHash);
    }

    [Fact]
    public void StreamDiff_Constructor_Null_HasNullHashes()
    {
        using var streamOld = new MemoryStream();
        using var streamNew = new MemoryStream();

        var diffOld = new StreamDiff(streamOld, null, true, true, CancellationToken.None);
        Assert.True(diffOld.HasChanged);
        Assert.NotNull(diffOld.OriginalHash);
        Assert.Null(diffOld.NewHash);

        var diffNew = new StreamDiff(old: null, streamNew, false, true, CancellationToken.None);
        Assert.True(diffOld.HasChanged);
        Assert.Null(diffNew.OriginalHash);
        Assert.NotNull(diffNew.NewHash);
    }

    [Fact]
    public void StreamDiff_HasChanged_True_Works()
    {
        Random rnd = new Random();
        for (int i = 0; i < 10; i++)
        {
            Byte[] bytesOld = new Byte[rnd.Next(2, 64)];
            rnd.NextBytes(bytesOld);
            Byte[] bytesNew = new Byte[rnd.Next(2, 64)];
            rnd.NextBytes(bytesNew);

            Assert.NotEqual(bytesOld, bytesNew);
            
            using var streamOld = new MemoryStream(bytesOld);
            using var streamNew = new MemoryStream(bytesNew);
            
            Assert.Equal(0, streamOld.Position);
            Assert.Equal(0, streamNew.Position);

            var diff = new StreamDiff(streamOld, streamNew, true, true, CancellationToken.None);
            Assert.True(diff.HasChanged);
            Assert.NotNull(diff.OriginalHash);
            Assert.NotNull(diff.NewHash);
        }
    }

    [Fact]
    public void StreamDiff_HasChanged_False_Works()
    {
        Random rnd = new Random();
        for (int i = 0; i < 10; i++)
        {
            Byte[] bytesOld = new Byte[rnd.Next(2, 64)];
            rnd.NextBytes(bytesOld);

            using var streamOld = new MemoryStream(bytesOld);
            using var streamNew = new MemoryStream(bytesOld);
            
            var diff = new StreamDiff(streamOld, streamNew, true, true, CancellationToken.None);
            Assert.False(diff.HasChanged);
            Assert.NotNull(diff.OriginalHash);
            Assert.NotNull(diff.NewHash);
        }
    }

    [Fact]
    public void StreamDiff_AreStreamsEqual_False_Works()
    {
        Random rnd = new Random();
        for (int i = 0; i < 10; i++)
        {
            Byte[] bytesOld = new Byte[rnd.Next(2, 64)];
            rnd.NextBytes(bytesOld);
            Byte[] bytesNew = new Byte[rnd.Next(2, 64)];
            rnd.NextBytes(bytesNew);

            Assert.NotEqual(bytesOld, bytesNew);
            
            using var streamOld = new MemoryStream(bytesOld);
            using var streamNew = new MemoryStream(bytesNew);
            
            Assert.False(StreamDiff.AreStreamsEqual(streamOld, streamNew, true, CancellationToken.None));
        }
    }

    [Fact]
    public void StreamDiff_AreStreamsEqual_True_Works()
    {
        Random rnd = new Random();
        for (int i = 0; i < 10; i++)
        {
            Byte[] bytesOld = new Byte[rnd.Next(2, 64)];
            rnd.NextBytes(bytesOld);

            using var streamOld = new MemoryStream(bytesOld);
            using var streamNew = new MemoryStream(bytesOld);
            
            Assert.True(StreamDiff.AreStreamsEqual(streamOld, streamNew, true, CancellationToken.None));
        }
    }
}