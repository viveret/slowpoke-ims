namespace slowpoke.core.Models.Identity;

public class PasswordIdentity: ISlowPokeIdentity
{
    public Guid IdentityGuid { get; }
    
    public Guid AuthGuid { get; }

    public string AuthAlg { get; }

    public string AuthKeyString { get; }

    public byte[] AuthKeyValue { get; }

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