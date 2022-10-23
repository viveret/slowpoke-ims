using System.Collections.Concurrent;
using System.Security.Principal;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Identity;

namespace slowpoke.core.Services.Identity;


public class IdentityAuthenticationService : IIdentityAuthenticationService
{
    private static readonly BiDirectional<Guid, Uri, ConcurrentDictionary<Guid, Uri>, ConcurrentDictionary<Uri, Guid>> CachedEndpoints = new();

    public Config Config { get; }

    public string CurrentIdentityPath => Path.Combine(Config.Paths.PrivateFilesPath, $"{nameof(CurrentIdentity)}.json");
    
    public ISlowPokeIdentity CurrentIdentity => File.Exists(CurrentIdentityPath) ? ParseIdentityModel(File.ReadAllText(CurrentIdentityPath)) : null;

    public IEnumerable<ISlowPokeIdentity> TrustedIdentities
    {
        get
        {
            return File.Exists(TrustedIdentitiesPath) ?
                        File.ReadAllLines(TrustedIdentitiesPath)
                            .Select(l => ParseIdentityModel(l)) :
                        Enumerable.Empty<ISlowPokeIdentity>();
        }
        set
        {
            File.WriteAllLines(TrustedIdentitiesPath, value.Select(v => SerializeIdentityModel(v)));
        }
    }

    public string TrustedIdentitiesPath => Path.Combine(Config.Paths.PrivateFilesPath, $"{nameof(TrustedIdentities)}.csv");

    public IEnumerable<ISlowPokeIdentity> UntrustedIdentities
    {
        get
        {
            return File.Exists(UntrustedIdentitiesPath) ?
                        File.ReadAllLines(UntrustedIdentitiesPath)
                            .Select(l => ParseIdentityModel(l)) :
                        Enumerable.Empty<ISlowPokeIdentity>();
        }
        set
        {
            File.WriteAllLines(UntrustedIdentitiesPath, value.Select(v => SerializeIdentityModel(v)));
        }
    }

    public string UntrustedIdentitiesPath => Path.Combine(Config.Paths.PrivateFilesPath, $"{nameof(UntrustedIdentities)}.csv");

    private ISlowPokeIdentity ParseIdentityModel(string l)
    {
        var model = System.Text.Json.JsonSerializer.Deserialize<IdentityModel>(l);
        if (model == null)
        {
            throw new Exception("model was null");
        }

        if (!string.IsNullOrWhiteSpace(model.AuthKeyString))
        {
            // need way to differentiate between site key (for HTTPS)
            // and user keys for encoding / decoding files / messages
            // like a keystore file 
            if (model.AuthKeyString.EndsWith("*.pfx"))
            {
                var dir = model.AuthKeyString.Substring(0, model.AuthKeyString.Length - "*.pfx".Length);
                model.AuthKeyString = System.IO.Directory.EnumerateFiles(dir, "*.pfx").FirstOrDefault() ?? string.Empty;
            }

            if (File.Exists(model.AuthKeyString))
            {
                model.AuthKeyValue = File.ReadAllBytes(model.AuthKeyString);
            }
        }
        
        return model;
    }

    private string SerializeIdentityModel(ISlowPokeIdentity v)
    {
        return System.Text.Json.JsonSerializer.Serialize(v);
    }

    public IdentityAuthenticationService(Config config)
    {
        Config = config;
    }

    public Uri GetEndpointForOriginGuid(Guid originGuid, CancellationToken cancellationToken)
    {
        return CachedEndpoints.Forward[originGuid];
    }

    public void SetEndpointForOriginGuid(Guid originGuid, Uri endpoint, CancellationToken cancellationToken)
    {
        var identity = GetIdentityFromOriginGuid(originGuid, cancellationToken);
        // return CachedEndpoints.Backward[originGuid];
    }

    public ISlowPokeIdentity GetIdentityFromOriginGuid(Guid originGuid, CancellationToken cancellationToken)
    {
        return new IdentityModel();
    }

    public ISlowPokeIdentity GetIdentityFromEndpoint(Uri endpoint, CancellationToken cancellationToken)
    {
        return new IdentityModel();
        // throw new NotImplementedException();
    }

    public TrustLevel GetTrustLevel(ISlowPokeIdentity identity, CancellationToken cancellationToken)
    {
        var isTrusted = TrustedIdentities.Any(other => other.AuthGuid == identity.AuthGuid &&
                                            other.IdentityGuid == identity.IdentityGuid &&
                                            other.AuthKeyEquals(identity));

        if (isTrusted)    
        {
            return TrustLevel.Trusted;
        }
        
        var isKnownButUntrusted = UntrustedIdentities.Any(other => other.AuthGuid == identity.AuthGuid &&
                                            other.IdentityGuid == identity.IdentityGuid &&
                                            other.AuthKeyEquals(identity));
        
        if (isKnownButUntrusted)
        {
            return TrustLevel.KnownButUntrusted;
        }

        return TrustLevel.Unknown;
    }

    public void SetTrustLevel(ISlowPokeIdentity identity, TrustLevel level, CancellationToken cancellationToken)
    {
        // remove from current/old list
        switch (GetTrustLevel(identity, cancellationToken))
        {
            case TrustLevel.Trusted:
            TrustedIdentities = TrustedIdentities.Where(ti => ti.AuthGuid != identity.AuthGuid);
            break;
            case TrustLevel.KnownButUntrusted:
            UntrustedIdentities = UntrustedIdentities.Where(ti => ti.AuthGuid != identity.AuthGuid);
            break;
            case TrustLevel.Unknown:
            break;
        }

        // append to new list
        switch (level)
        {
            case TrustLevel.Trusted:
            TrustedIdentities = TrustedIdentities.Where(ti => ti.AuthGuid != identity.AuthGuid);
            break;
            case TrustLevel.KnownButUntrusted:
            UntrustedIdentities = UntrustedIdentities.Where(ti => ti.AuthGuid != identity.AuthGuid);
            break;
            case TrustLevel.Unknown:
            break;
        }
    }

    public ISlowPokeIdentity GetIdentityFromAuthGuid(Guid authGuid, CancellationToken cancellationToken)
    {
        return TrustedIdentities.Concat(UntrustedIdentities).Where(i => i.AuthGuid == authGuid).FirstOrDefault();
    }
}