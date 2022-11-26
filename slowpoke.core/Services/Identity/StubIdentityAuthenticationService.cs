using slowpoke.core.Models.Identity;

namespace slowpoke.core.Services.Identity;


public class StubIdentityAuthenticationService : IIdentityAuthenticationService
{
    public ISlowPokeIdentity CurrentIdentity => new IdentityModel();

    public IEnumerable<ISlowPokeIdentity> TrustedIdentities
    {
        get
        {
            return Enumerable.Empty<ISlowPokeIdentity>();
        }
        set { }
    }

    public IEnumerable<ISlowPokeIdentity> UntrustedIdentities
    {
        get
        {
            return Enumerable.Empty<ISlowPokeIdentity>();
        }
        set { }
    }

    public StubIdentityAuthenticationService() { }

    public Uri GetEndpointForOriginGuid(Guid originGuid, CancellationToken cancellationToken) => throw new NotImplementedException();

    public void SetEndpointForOriginGuid(Guid originGuid, Uri? endpoint, CancellationToken cancellationToken) { }

    public ISlowPokeIdentity GetIdentityFromOriginGuid(Guid originGuid, CancellationToken cancellationToken) => new IdentityModel();

    public ISlowPokeIdentity GetIdentityFromEndpoint(Uri endpoint, CancellationToken cancellationToken) => new IdentityModel();

    public TrustLevel GetTrustLevel(ISlowPokeIdentity identity, CancellationToken cancellationToken) => TrustLevel.Unknown;

    public void SetTrustLevel(ISlowPokeIdentity identity, TrustLevel? level, CancellationToken cancellationToken) { }

    public ISlowPokeIdentity GetIdentityFromAuthGuid(Guid authGuid, CancellationToken cancellationToken) => new IdentityModel();
}