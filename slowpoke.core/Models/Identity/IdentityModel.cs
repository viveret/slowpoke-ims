namespace slowpoke.core.Models.Identity;



public class IdentityModel : ISlowPokeIdentity
{
    public IdentityModel()
    {
    }

    public IdentityModel(Guid identityGuid, Guid authGuid, string authAlg, string authKeyString, byte[] authKeyValue)
    {
        IdentityGuid = identityGuid;
        AuthGuid = authGuid;
        AuthAlg = authAlg;
        AuthKeyString = authKeyString;
        AuthKeyValue = authKeyValue;
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
}