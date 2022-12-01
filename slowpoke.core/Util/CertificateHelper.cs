using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace slowpoke.core.Util
{
    public static class CertificateHelper
    {
        public static async Task<X509Certificate2> GetCertificate(string url)
        {
            if (url.StartsWith("https://"))
            {
                url = url.Substring("https://".Length);
            }
            else if (url.StartsWith("http://"))
            {
                url = url.Substring("http://".Length);
            }
            
            RemoteCertificateValidationCallback certCallback = (_, _, _, _) => true;
            using var client = new TcpClient(url, 443);
            using var sslStream = new SslStream(client.GetStream(), true, certCallback);
            await sslStream.AuthenticateAsClientAsync(url);
            var serverCertificate = sslStream.RemoteCertificate ?? throw new Exception("RemoteCertificate not found");
            return new X509Certificate2(serverCertificate);
        }
    }
}