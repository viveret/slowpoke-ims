using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using slowpoke.core.Client;

namespace SlowPokeIMS.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // need to handle this a lot better
            HttpSlowPokeClient.SystemCertificate = new X509Certificate2(pfxPath);
            CreateWebHostBuilder(args).Build().Run();
        }

        private static string pfxPath => System.IO.Directory.GetFiles("/home/viveret/.dotnet/corefx/cryptography/x509stores/my/", "*.pfx").First();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options: options =>
                {
                   options.Listen(IPAddress.Loopback, 5000);
                   if (File.Exists(pfxPath))
                   {
                       options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                       {
                           listenOptions.UseHttps(HttpSlowPokeClient.SystemCertificate);
                       });
                   }
                })
                .UseStartup<Startup>();
    }
}