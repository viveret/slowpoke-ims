using System;
using System.Threading;
using System.Threading.Tasks;
using slowpoke.core.Client;
using slowpoke.core.Models.SyncState;
using SlowPokeIMS.Integration.Tests.Core;
using Xunit;

namespace SlowPokeIMS.Core.Integration.Tests.Client;


public class HttpSlowPokeClientIntegrationTests: IClassFixture<WebServerFixture>
{
    private readonly WebServerFixture fixture;

    public HttpSlowPokeClientIntegrationTests(WebServerFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task HttpSlowPokeClient_Ping_Works()
    {
        var client = await fixture.GetClient1();
        Assert.True(await client.Ping(CancellationToken.None));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task HttpSlowPokeClient_Sync_Works(bool asynchronous, bool immediately)
    {
        Assert.True(await fixture.AddServersToTrusted(true, true));
        var client = await fixture.GetClient1();
        var syncResult = await client.Sync(asynchronous: asynchronous, immediately: immediately);
        Assert.True(syncResult);
    }

    [Fact]
    public async Task HttpSlowPokeClient_IsUpToDate_Works()
    {
        var client = await fixture.GetClient1();
        Assert.False(await client.IsUpToDate());
    }

    [Fact]
    public async Task HttpSlowPokeClient_GetSyncState_Works()
    {
        var client = await fixture.GetClient1();
        Assert.NotEqual(SyncState.ErrorState, await client.GetSyncState(CancellationToken.None));
        Assert.NotEqual(SyncState.OutOfDate, await client.GetSyncState(CancellationToken.None));
        Assert.NotEqual(SyncState.Unknown, await client.GetSyncState(CancellationToken.None));
    }
}