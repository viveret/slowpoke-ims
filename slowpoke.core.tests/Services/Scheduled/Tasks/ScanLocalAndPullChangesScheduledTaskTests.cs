using Xunit;

using slowpoke.core.Services;
using slowpoke.core.Services.Scheduled;
using slowpoke.core.Services.Scheduled.Tasks;
using slowpoke.core.Services.Node.Docs;
using slowpoke.core.Models.Configuration;

namespace SlowPokeIMS.Core.Tests.Services.Scheduled.Tasks;

public class ScanLocalAndPullChangesScheduledTaskTests
{
    [Fact]
    public void ScanLocalAndPullChangesScheduledTask_Constructor_Default()
    {
        var cfg = new Config();
        var task = new ScanLocalAndPullChangesScheduledTask(cfg, new SyncStateManager(), null, new StubDocumentProviderResolver(cfg));
    }

    [Fact]
    public void ScanLocalAndPullChangesScheduledTask_CreateContext()
    {
        var cfg = new Config();
        var task = new ScanLocalAndPullChangesScheduledTask(cfg, new SyncStateManager(), null, new StubDocumentProviderResolver(cfg));
        Assert.NotNull(task.CreateContext(ScheduledTaskManager.Empty()));
    }
}