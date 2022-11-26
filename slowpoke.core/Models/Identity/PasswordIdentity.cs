namespace slowpoke.core.Models.Identity;

public class PasswordIdentity: ISlowPokeIdentity
{
    public Guid IdentityGuid { get; set; }
    
    public Guid AuthGuid { get; set; }

    public string AuthAlg { get; set; } = string.Empty;

    public string AuthKeyString { get; set; } = string.Empty;

    public byte[] AuthKeyValue { get; set; } = Array.Empty<byte>();

    public bool Exists => false;

    public bool AuthKeyEquals(ISlowPokeIdentity other)
    {
        if (other is PasswordIdentity passwordIdentity)
        {
            return false; // ClearTextPassword == passwordIdentity.ClearTextPassword;
        }
        else
        {
            return false;
        }
    }
}