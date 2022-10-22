using System.Security.Cryptography;

namespace slowpoke.core.Extensions;


public static class StreamExtensions
{
    public static string ComputeMD5FromStream(this Stream stream)
    {
        // https://stackoverflow.com/a/43647643/11765486
        using (var md5 = MD5.Create()) {
            var hash = md5.ComputeHash(stream);
            var base64String = Convert.ToBase64String(hash);
            return base64String;
        }
    }
}