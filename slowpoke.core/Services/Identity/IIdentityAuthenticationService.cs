using System.Security.Principal;
using slowpoke.core.Models.Identity;

namespace slowpoke.core.Services.Identity;


public interface IIdentityAuthenticationService
{
    ISlowPokeIdentity CurrentIdentity { get; }
    
    IEnumerable<ISlowPokeIdentity> TrustedIdentities { get; }
    
    IEnumerable<ISlowPokeIdentity> UntrustedIdentities { get; }

    Uri? GetEndpointForOriginGuid(Guid originGuid, CancellationToken cancellationToken);

    void SetEndpointForOriginGuid(Guid originGuid, Uri? endpoint, CancellationToken cancellationToken);

    ISlowPokeIdentity GetIdentityFromAuthGuid(Guid authGuid, CancellationToken cancellationToken);
    
    ISlowPokeIdentity GetIdentityFromOriginGuid(Guid originGuid, CancellationToken cancellationToken);
    
    ISlowPokeIdentity GetIdentityFromEndpoint(Uri endpoint, CancellationToken cancellationToken);

    TrustLevel GetTrustLevel(ISlowPokeIdentity identity, CancellationToken cancellationToken);
    
    void SetTrustLevel(ISlowPokeIdentity identity, TrustLevel? level, CancellationToken cancellationToken);
}