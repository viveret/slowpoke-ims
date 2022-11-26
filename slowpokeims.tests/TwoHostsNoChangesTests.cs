using System;
using System.Threading.Tasks;
using slowpoke.core.Models.SyncState;
using SlowPokeIMS.Integration.Tests.Core;
using Xunit;

namespace SlowPokeIMS.Tests;

// This does not work because the hosts don't actually know about each other.
// need to call configure function on each host that adds reference to the other
// so that they are aware of each other and can use the test slow poke server fixture's client
// for sending and receiving requests. trying to open a new client will not work.
public class TwoHostsNoChangesTests: IClassFixture<WebServerFixture>
{
    private static void CreateFoldersOnClients(ITestSlowPokeClient client1, ITestSlowPokeClient client2, int count = 7)
    {
        for (int i = 1; i <= count; i++)
        {
            var folderPath = $"~/folder-to-sync-{i}";
            client1.CreateFolder(folderPath, true);
            client2.CreateFolder(folderPath, true);
        }
    }

    private static void CreateFilesOnClients(ITestSlowPokeClient client1, ITestSlowPokeClient client2, int count = 7, string fileContent1 = "", string fileContent2 = "")
    {
        for (int i = 1; i <= count; i++)
        {
            var filePath = $"~/file-to-sync-{i}.txt";
            client1.CreateFile(filePath, fileContent1, true);
            client2.CreateFile(filePath, fileContent2, true);
        }
    }


    private readonly WebServerFixture fixture;

    // need setup (maybe teardown) method for hosts, doc providers, in memory files, etc
    // might need to have multiple kestrel servers in order to automate HTTP client
    // connections in order to achieve END TO END testing (full coverage integration testing)
    public TwoHostsNoChangesTests(WebServerFixture fixture)
    {
        this.fixture = fixture;
    }

    // need to rewrite to not use Http, use generated client from TestStartup
    protected async Task RunTestOnTwoHosts(
        Action<ITestSlowPokeClient, ITestSlowPokeClient>? setupFilesAndFolders,
        Action<ITestSlowPokeClient, ITestSlowPokeClient>? test,
        bool testNoChanges = false)
    {
        Assert.True(await fixture.AddServersToTrusted(true, true));
        
        var client1 = await fixture.GetTestClient1();
        var client2 = await fixture.GetTestClient2();

        Assert.True(await client1.Ping(System.Threading.CancellationToken.None));
        Assert.True(await client2.Ping(System.Threading.CancellationToken.None));
        
        // start from empty state
        await client1.EnsureNoFilesOrFolders();
        await client2.EnsureNoFilesOrFolders();
        
        setupFilesAndFolders?.Invoke(client1, client2);

        Assert.True(await client1.Sync(asynchronous: false, immediately: true));
        Assert.True(await client2.Sync(asynchronous: false, immediately: true));
        
        test?.Invoke(client1, client2);

        if (testNoChanges)
        {
            Assert.Equal(SyncState.UpToDate, await client1.GetSyncState(System.Threading.CancellationToken.None));
            Assert.Equal(SyncState.UpToDate, await client2.GetSyncState(System.Threading.CancellationToken.None));
        }
    }

    [Fact]
    public async void HostsHaveNoFilesOrFoldersNoChangesWorks()
    {
        // 2 hosts that are empty should always report no changes
        await RunTestOnTwoHosts(null, null, testNoChanges: true);
    }

    [Fact]
    public async void HostsHaveNoFilesOneFolderNoChangesWorks()
    {
        // 2 hosts that are empty except one folder should always report no changes
        // as long as the folder does not change
        await RunTestOnTwoHosts((client1, client2) => {
            var folderPath = "~/folder-to-sync";
            client1.CreateFolder(folderPath, true);
            client2.CreateFolder(folderPath, true);
        }, null, testNoChanges: true);
    }
    
    [Fact]
    public async void HostsHaveNoFilesMultipleFoldersNoChangesWorks()
    {
        // 2 hosts that are empty except a few folders should always report no changes
        // as long as the folders do not change
        await RunTestOnTwoHosts((client1, client2) => {
            for (int i = 1; i <= 7; i++)
            {
                var folderPath = $"~/folder-to-sync-{i}";
                client1.CreateFolder(folderPath, true);
                client2.CreateFolder(folderPath, true);
            }
        }, null, testNoChanges: true);
    }
    
    [Fact]
    public async void HostsHaveOneFileNoFoldersNoChangesWorks()
    {
        // 2 hosts that are empty except for one file should always report no changes
        // as long as the file does not change
        await RunTestOnTwoHosts((client1, client2) =>
        {
            var filePath = $"~/file-to-sync";
            client1.CreateFile(filePath, "", true);
            client2.CreateFile(filePath, "", true);
        }, null, testNoChanges: true);
    }

    [Fact]
    public async void HostsHaveOneFileOneFolderNoChangesWorks()
    {
        // 2 hosts that are empty should always report no changes
        // as long as the file and folder do not change
        await RunTestOnTwoHosts((client1, client2) => {
            var filePath = $"~/file-to-sync";
            client1.CreateFile(filePath, "", true);
            client2.CreateFile(filePath, "", true);
            
            var folderPath = $"~/folder-to-sync";
            client1.CreateFolder(folderPath, true);
            client2.CreateFolder(folderPath, true);
        }, null, testNoChanges: true);
    }
    
    [Fact]
    public async void HostsHaveOneFileMultipleFoldersNoChangesWorks()
    {
        // 2 hosts that are empty should always report no changes
        // as long as the one file and multiple folders do not change
        await RunTestOnTwoHosts((client1, client2) =>
        {
            var filePath = $"~/file-to-sync";
            client1.CreateFile(filePath, "", true);
            client2.CreateFile(filePath, "", true);

            CreateFoldersOnClients(client1, client2);
        }, null, testNoChanges: true);
    }

    [Fact]
    public async void HostsHaveMultipleFilesNoFoldersNoChangesWorks()
    {
        // 2 hosts that are empty should always report no changes
        // as long as the files do not change
        await RunTestOnTwoHosts((client1, client2) => {
            CreateFilesOnClients(client1, client2);
        }, null, testNoChanges: true);
    }
    
    [Fact]
    public async void HostsHaveMultipleFilesOneFolderNoChangesWorks()
    {
        // 2 hosts that are empty should always report no changes
        // as long as the files and folder do not change
        await RunTestOnTwoHosts((client1, client2) => {
            CreateFilesOnClients(client1, client2);
            
            var folderPath = $"~/folder-to-sync";
            client1.CreateFolder(folderPath, true);
            client2.CreateFolder(folderPath, true);
        }, null, testNoChanges: true);
    }
    
    [Fact]
    public async void HostsHaveMultipleFilesAndFoldersNoChangesWorks()
    {
        // 2 hosts that are empty should always report no changes
        // as long as the files and folders do not change
        await RunTestOnTwoHosts((client1, client2) => {
            CreateFilesOnClients(client1, client2);
            CreateFoldersOnClients(client1, client2);
        }, null, testNoChanges: true);
    }
}