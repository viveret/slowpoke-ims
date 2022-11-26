using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Client;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node;

namespace SlowPokeIMS.Integration.Tests.Core;


public class TestSlowPokeServer: IDisposable
{
    //private const int ServerPortHttp = 3001;
    //private const int ServerPortHttps = 3002;
    //private string ServerUrl => $"https://127.0.0.1:{ServerPortHttps}/";
    
    private TestServer _server;
    
    public Config ClientConfig { get; }

    private TestSlowPokeServer(Config clientConfig)
    {
        ClientConfig = clientConfig;
        //_factory = new WebApplicationFactory<TStartup>();
        //_factory = new TestHostBuilder();
    }

    private void CreateServer<TStartup>(TestServerFixture<TStartup> testServerFixture) where TStartup: class
    {
        void ConfigureServicesForTestServer(IServiceCollection obj)
        {
            obj.AddSingleton(testServerFixture);
        }
        _server = new TestServer(new WebHostBuilder().ConfigureServices(ConfigureServicesForTestServer).UseStartup<TStartup>());
    }

    private IWebHost? Server;
    private ISlowPokeClient? Client;
    private ITestSlowPokeClient? TestClient;


    public HttpClient GetHttpClient()
    {
        // return Client1 ??= CreateClient(Server1Url);
        return _server.CreateClient();
    }

    public async Task<ISlowPokeClient> GetClient()
    {
        // return Client1 ??= CreateClient(Server1Url);
        return Client ??= await HttpSlowPokeClient.CreateClient(_server.BaseAddress, ClientConfig, GetHttpClient());// new Uri(ServerUrl)
    }

    public async Task<ITestSlowPokeClient> GetTestClient()
    {
        // return TestClient1 ??= new HttpTestSlowPokeClient(new Uri(Server1Url), ClientConfig);
        return TestClient ??= await HttpTestSlowPokeClient.CreateTestClient(_server.BaseAddress, ClientConfig, _server.CreateClient());
    }


    public void Stop() {}
    
    public void Dispose() {}

    public static TestSlowPokeServer Create<TStartup>(Config clientConfig, TestServerFixture<TStartup> testServerFixture) where TStartup: class
    {
        var ret = new TestSlowPokeServer(clientConfig);
        ret.CreateServer<TStartup>(testServerFixture);
        return ret;
    }

    public ISlowPokeHost ToHostModel()
    {
        return new SlowPokeHostModel() { Endpoint = Client!.Endpoint };
    }


    //     public IConfiguration? Config { get; set; }

    //     public IWebHost Host { get; set; }

    //     public int PortHttp { get; set; }
    //     public int PortHttps { get; set; }
    //     public X509Certificate2? HttpsCertificate { get; set; }

    //     public string Url => $"https://127.0.0.1:{PortHttps}/";

    //     public void Stop() => Host.StopAsync();

    //     public void Dispose() => Host.Dispose();
}