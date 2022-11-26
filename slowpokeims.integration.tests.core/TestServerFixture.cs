using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Security;
using slowpoke.core.Client;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Util;


namespace SlowPokeIMS.Integration.Tests.Core;


public class TestServerFixture<TStartup> : IDisposable where TStartup: class
{
    private static readonly CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
    private static readonly SecureRandom random = new SecureRandom(randomGenerator);

    private readonly TestSlowPokeServer _server1;
    private readonly TestSlowPokeServer _server2;
    private readonly Config ClientConfig;
    
    public List<TestSlowPokeServer> Servers { get; } = new List<TestSlowPokeServer>();

    public TestServerFixture()
    {
        ClientConfig = new Config();
        _server1 = TestSlowPokeServer.Create<TStartup>(ClientConfig, this);
        _server2 = TestSlowPokeServer.Create<TStartup>(ClientConfig, this);
    }

    public async Task<bool> AddServersToTrusted(bool server1, bool server2)
    {
        var testClient1 = await _server1.GetTestClient();
        var testClient2 = await _server2.GetTestClient();

        var added1 = await testClient1.AddTrusted($"https://automatedtest.local.client/2", true, CancellationToken.None);
        var added2 = await testClient2.AddTrusted($"https://automatedtest.local.client/1", true, CancellationToken.None);

        return added1 && added2;
    }

    public X509Certificate2 GenerateCertificate(string url)
    {
        var certPath = Path.GetTempFileName();
        var password = Guid.NewGuid().ToString();
        var commonName = url;
        var rsaKeySize = 2048;
        var years = 5;
        var hashAlgorithm = HashAlgorithmName.SHA512;

        using (var rsa = RSA.Create(rsaKeySize))
        {
            var request = new CertificateRequest($"cn={commonName}", rsa, hashAlgorithm, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false)
            );

            var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(years));

            // Return the PFX exported version that contains the key
            return new X509Certificate2(certificate.Export(X509ContentType.Pfx, password), password, X509KeyStorageFlags.MachineKeySet);
        }
    }

    public HttpClient GetHttpClient1() => _server1.GetHttpClient();

    public Task<ISlowPokeClient> GetClient1() => _server1.GetClient();

    public Task<ITestSlowPokeClient> GetTestClient1() => _server1.GetTestClient();

    public Task<ISlowPokeClient> GetClient2() => _server2.GetClient();

    public Task<ITestSlowPokeClient> GetTestClient2() => _server2.GetTestClient();

    // public IWebHost CreateWebHostWithTestStartup(int portHttp, int portHttps, X509Certificate2 certificate, IConfigurationRoot Config)
    // {
    //     return new WebHostBuilder()
    //         .UseKestrel(options: options =>
    //         {
    //             options.ListenLocalhost(portHttp, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2);
    //             options.ListenLocalhost(portHttps, listenOptions =>
    //             {
    //                 listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    //                 listenOptions.UseHttps(certificate);
    //             });
    //         })
    //         .UseConfiguration(Config)
    //         .ConfigureServices(services => {
    //             // services.Add(new ServiceDescriptor(typeof(WebServerFixture), _ => this, ServiceLifetime.Singleton));
    //         })
    //         .UseStartup<TStartup>()
    //         .Build();
    // }

    public IConfigurationRoot CreateConfig()
    {
        var cfg = new Dictionary<string, string>
        {
            ["IsForAutomatedTests"] = "true",
            //["ASPNETCORE_ENVIRONMENT"] = "Development",
        };

        return
            new ConfigurationBuilder()
                .AddInMemoryCollection(cfg)
                .Build();
    }

    public void Dispose()
    {
        foreach (var server in Servers)
        {
            server.Stop();
            server.Dispose();
        }
    }
}

public class TestServerFixture: TestServerFixture<TestStartup>
{

}