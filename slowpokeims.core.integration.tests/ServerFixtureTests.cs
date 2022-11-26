using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Client;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Integration.Tests.Core;
using SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;
using Xunit;

namespace SlowPokeIMS.Core.Integration.Tests;

public class ServerFixtureTests
{
    [Fact]
    public void ServerFixture_Constructor_Works()
    {
        new TestServerFixture();
    }
    
    [Fact]
    public void ServerFixture_GenerateCertificate_Works()
    {
        Assert.NotNull(new TestServerFixture().GenerateCertificate("https://127.0.0.1"));
    }
    
    [Fact]
    public void ServerFixture_CreateConfig_Works()
    {
        Assert.NotNull(new TestServerFixture().CreateConfig());
    }
    
    // [Fact]
    // public void ServerFixture_CreateWebHostWithTestStartup_Works()
    // {
    //     var fixture = new TestServerFixture();
    //     var cfg = fixture.CreateConfig();
    //     Assert.NotNull(fixture.CreateWebHostWithTestStartup(80, 443, fixture.GenerateCertificate($"https://127.0.0.1"), cfg));
    // }
    
    // [Fact]
    // public async Task ServerFixture_Start_WebHost_Works()
    // {
    //     var fixture = new TestServerFixture();
    //     var cfg = fixture.CreateConfig();
    //     var webhost = fixture.CreateWebHostWithTestStartup(8080, 8443, fixture.GenerateCertificate($"https://127.0.0.1"), cfg);
    //     webhost.Start();

    //     var docProvider = webhost.Services.GetRequiredService<IDocumentProviderResolver>();
    //     Assert.NotNull(docProvider);

    //     var repoResolver = await docProvider.ReadLocal;
    //     Assert.IsType<GenericReadWriteDocumentResolver>(repoResolver);
    //     Assert.NotNull(((GenericReadOnlyDocumentResolver)repoResolver).inMemoryGenericDocumentRepository);

    //     await webhost.StopAsync();
    // }
}