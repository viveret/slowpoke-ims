using System.Security.Cryptography.X509Certificates;

namespace slowpoke.core.Models.Identity;



public static class ISlowPokeIdentityExtensions
{
    public static X509Certificate2 ConvertToX509Cert(this ISlowPokeIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (identity.AuthAlg != "X.509")
        {
            throw new ArgumentException("", nameof(identity));
        }

        return new X509Certificate2(identity.AuthKeyValue);
    }
}