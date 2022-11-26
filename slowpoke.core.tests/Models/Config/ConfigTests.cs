using System;
using System.Threading;
using Xunit;

using slowpoke.core.Models.Configuration;

namespace SlowPokeIMS.Core.Tests.Models.Configuration;

public class ConfigTests
{
    [Fact]
    public void ConfigConstructorDefault()
    {
        var msg = new Config();
    }

    [Fact]
    public void ConfigDefaultAssignmentWorks()
    {
        var c = new Config();
        c.SetDefaults();
    }

    [Fact]
    public void ConfigSaveWorks()
    {
        var c = new Config();
        c.Save(CancellationToken.None);
    }

    [Fact]
    public void ConfigLoadWorks()
    {
        var c = new Config();
        c.Load(CancellationToken.None);
    }
}