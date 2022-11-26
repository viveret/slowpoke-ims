using System.Collections.Concurrent;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Identity;
using slowpoke.core.Services.Identity;

namespace SlowPokeIMS.Tests.Core.Services;


public class TestIdentityAuthenticationService : IIdentityAuthenticationService
{
    public readonly ConcurrentDictionary<Guid, ISlowPokeIdentity> Identities = new ();
    public readonly ConcurrentDictionary<Guid, ISlowPokeIdentity> IdentitiesByAuthGuid = new ();
    public readonly ConcurrentDictionary<Guid, ISlowPokeIdentity> IdentitiesByOriginGuid = new ();
    public readonly ConcurrentDictionary<Uri, ISlowPokeIdentity> IdentitiesByEndpoint = new ();
    public readonly ConcurrentDictionary<Guid, Uri> EndpointsByOriginGuid = new ();
    public readonly ConcurrentDictionary<Guid, TrustLevel> TrustLevelsByIdentityGuid = new ();
    
    private IdentityModel? _currentIdentity = null;
    private readonly byte[] _currentIdentityAuthBytes = new byte[] { 0x1, 0x2, 0x3 };

    public Config Config { get; }

    public TestIdentityAuthenticationService(Config config)
    {
        Config = config;
    }

    public ISlowPokeIdentity CurrentIdentity => _currentIdentity ??= new IdentityModel(Guid.NewGuid(), Guid.NewGuid(), "X.509", "", _currentIdentityAuthBytes); // /home/viveret/.dotnet/corefx/cryptography/x509stores/my/*.pfx

    public IEnumerable<ISlowPokeIdentity> TrustedIdentities => TrustLevelsByIdentityGuid.Where(kvp => kvp.Value == TrustLevel.Trusted).Select(v => GetIdentity(v.Key));

    public IEnumerable<ISlowPokeIdentity> UntrustedIdentities => TrustLevelsByIdentityGuid.Where(kvp => kvp.Value == TrustLevel.KnownButUntrusted).Select(v => GetIdentity(v.Key));

    private ISlowPokeIdentity GetIdentity(Guid key)
    {
        return Identities.TryGetValue(key, out var identity) ? identity : IdentityModel.DoesNotExist;
    }

    public void AddIdentity(ISlowPokeIdentity identity, Guid originGuid, Uri endpoint, TrustLevel trustLevel)
    {
        Identities[identity.IdentityGuid] = identity;
        IdentitiesByAuthGuid[identity.AuthGuid] = identity;
        IdentitiesByOriginGuid[originGuid] = identity;
        IdentitiesByEndpoint[endpoint] = identity;
        TrustLevelsByIdentityGuid[identity.IdentityGuid] = trustLevel;
    }

    public Uri? GetEndpointForOriginGuid(Guid originGuid, CancellationToken cancellationToken)
    {
        return EndpointsByOriginGuid.TryGetValue(originGuid, out var uri) ? uri : null;
    }

    public void SetEndpointForOriginGuid(Guid originGuid, Uri? endpoint, CancellationToken cancellationToken)
    {
        if (endpoint != null)
        {
            EndpointsByOriginGuid[originGuid] = endpoint;
        }
        else
        {
            EndpointsByOriginGuid.Remove(originGuid, out var _);
        }
    }

    public ISlowPokeIdentity GetIdentityFromAuthGuid(Guid authGuid, CancellationToken cancellationToken)
    {
        return IdentitiesByAuthGuid.TryGetValue(authGuid, out var identity) ? identity : IdentityModel.DoesNotExist;
    }

    public ISlowPokeIdentity GetIdentityFromOriginGuid(Guid originGuid, CancellationToken cancellationToken)
    {
        return IdentitiesByOriginGuid.TryGetValue(originGuid, out var identity) ? identity : IdentityModel.DoesNotExist;
    }

    public ISlowPokeIdentity GetIdentityFromEndpoint(Uri endpoint, CancellationToken cancellationToken)
    {
        return IdentitiesByEndpoint.TryGetValue(endpoint, out var identity) ? identity : IdentityModel.DoesNotExist;
    }

    public TrustLevel GetTrustLevel(ISlowPokeIdentity identity, CancellationToken cancellationToken)
    {
        if (identity == null)
        {
            throw new ArgumentNullException(nameof(identity));
        }
        else if (ReferenceEquals(CurrentIdentity, identity) || identity.IdentityGuid == CurrentIdentity.IdentityGuid)
        {
            return TrustLevel.Trusted;
        }
        else
        {
            return TrustLevelsByIdentityGuid.TryGetValue(identity.IdentityGuid, out var trustLevel) ? trustLevel : TrustLevel.Unknown;
        }
    }

    public void SetTrustLevel(ISlowPokeIdentity identity, TrustLevel? level, CancellationToken cancellationToken)
    {
        if (level.HasValue)
        {
            TrustLevelsByIdentityGuid.AddOrUpdate(identity.IdentityGuid, (_) => level.Value, (_, _) => level.Value);
        }
        else
        {
            TrustLevelsByIdentityGuid.Remove(identity.IdentityGuid, out var _);
        }
    }
}