using System;
using Xunit;

using slowpoke.core.Models.Broadcast;
using SlowPokeIMS.Core.Util;

namespace SlowPokeIMS.Core.Tests.Models.Broadcast;

public class BroadcastMessageRawTests
{
    [Fact]
    public void BroadcastMessageRawConstructorDefault()
    {
        var msg = new BroadcastMessageRaw();
    }

    [Fact]
    public void BroadcastMessageRawCannotParseEmptyString()
    {
        Assert.Throws<ArgumentNullException>(() => BroadcastMessageRaw.Parse(null));
        Assert.Throws<ArgumentNullException>(() => BroadcastMessageRaw.Parse(string.Empty));
    }

    [Fact]
    public void BroadcastMessageRawCannotParseRandomString()
    {
        Assert.Throws<ArgumentException>(() => BroadcastMessageRaw.Parse(TestUtil.GenerateRandomString()));
    }
}