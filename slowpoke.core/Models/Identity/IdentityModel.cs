namespace slowpoke.core.Models.Identity;



public class IdentityModel : ISlowPokeIdentity
{
    public IdentityModel():
        this(Guid.Empty, Guid.Empty, string.Empty, string.Empty, Array.Empty<byte>())
    { }

    public IdentityModel(ISlowPokeIdentity other):
        this(other.IdentityGuid, other.AuthGuid, other.AuthAlg, other.AuthKeyString, other.AuthKeyValue)
    { }

    public IdentityModel(Guid identityGuid, Guid authGuid, string authAlg, string authKeyString, byte[] authKeyValue)
    {
        IdentityGuid = identityGuid;
        AuthGuid = authGuid;
        AuthAlg = authAlg;
        AuthKeyString = authKeyString;
        AuthKeyValue = authKeyValue;
        Exists = true;
    }

    public Guid IdentityGuid { get; set; }

    public Guid AuthGuid { get; set; }

    public string AuthAlg { get; set; }

    public string AuthKeyString { get; set; }

    public byte[] AuthKeyValue { get; set; }

    public bool AuthKeyEquals(ISlowPokeIdentity other)
    {
        return AuthAlg == other.AuthAlg && Array.Equals(AuthKeyValue, other.AuthKeyValue);
    }

    public static ISlowPokeIdentity DoesNotExist { get; } = new IdentityModel { Exists = false };

    public bool Exists { get; private set; }
}