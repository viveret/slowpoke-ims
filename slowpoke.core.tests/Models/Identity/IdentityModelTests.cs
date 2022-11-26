using System;
using Xunit;

using slowpoke.core.Models.Broadcast;

namespace SlowPokeIMS.Core.Tests;

public class IdentityModelTests
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
}