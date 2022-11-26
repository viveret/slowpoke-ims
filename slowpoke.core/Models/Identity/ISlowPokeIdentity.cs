namespace slowpoke.core.Models.Identity;

public interface ISlowPokeIdentity
{
    bool Exists { get; }
    
    Guid IdentityGuid { get; }
    
    Guid AuthGuid { get; }

    string AuthAlg { get; }

    string AuthKeyString { get; }

    byte[] AuthKeyValue { get; }

    bool AuthKeyEquals(ISlowPokeIdentity other);
}