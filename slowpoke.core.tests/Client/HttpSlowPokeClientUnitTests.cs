using System;
using System.Threading;
using System.Threading.Tasks;
using slowpoke.core.Client;
using Xunit;

namespace SlowPokeIMS.Core.Tests.Client;



public class HttpSlowPokeClientUnitTests
{
    [Fact]
    public async Task HttpSlowPokeClient_Constructor_Works()
    {
        await HttpSlowPokeClient.CreateClient(new Uri("https://127.0.0.1"), new slowpoke.core.Models.Configuration.Config(), requireConnection: false);
    }
    
    [Fact]
    public Task HttpSlowPokeClient_Constructor_Null_Uri_Exception()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => HttpSlowPokeClient.CreateClient(null, new slowpoke.core.Models.Configuration.Config()));
        return Task.CompletedTask;
    }

    [Fact]
    public Task HttpSlowPokeClient_Constructor_Null_Config_Exception()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => HttpSlowPokeClient.CreateClient(new Uri("https://127.0.0.1"), null));
        return Task.CompletedTask;
    }
}