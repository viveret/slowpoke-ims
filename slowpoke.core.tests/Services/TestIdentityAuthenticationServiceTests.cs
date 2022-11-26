using System;
using Xunit;

using slowpoke.core.Models.Configuration;
using SlowPokeIMS.Tests.Core.Services;
using System.Threading;
using slowpoke.core.Models.Identity;

namespace SlowPokeIMS.Core.Tests;

public class TestIdentityAuthenticationServiceTests
{
    [Fact]
    public void TestIdentityAuthenticationService_Constructor_Works()
    {
        var cfg = new Config();
        new TestIdentityAuthenticationService(cfg);
    }

    // Uri GetEndpointForOriginGuid(Guid originGuid, CancellationToken cancellationToken);

    // void SetEndpointForOriginGuid(Guid originGuid, Uri endpoint, CancellationToken cancellationToken);

    // ISlowPokeIdentity GetIdentityFromAuthGuid(Guid authGuid, CancellationToken cancellationToken);
    
    // ISlowPokeIdentity GetIdentityFromOriginGuid(Guid originGuid, CancellationToken cancellationToken);
    
    // ISlowPokeIdentity GetIdentityFromEndpoint(Uri endpoint, CancellationToken cancellationToken);

    // TrustLevel GetTrustLevel(ISlowPokeIdentity identity, CancellationToken cancellationToken);
    
    // void SetTrustLevel(ISlowPokeIdentity identity, TrustLevel level, CancellationToken cancellationToken);
    
    [Fact]
    public void TestIdentityAuthenticationService_CurrentIdentity_Works()
    {
        var cfg = new Config();
        var service = new TestIdentityAuthenticationService(cfg);
        Assert.NotNull(service.CurrentIdentity);
        Assert.NotEqual(Guid.Empty, service.CurrentIdentity.AuthGuid);
        Assert.NotEqual(Guid.Empty, service.CurrentIdentity.IdentityGuid);
        Assert.NotNull(service.CurrentIdentity.AuthAlg);
        Assert.NotEmpty(service.CurrentIdentity.AuthAlg);
        Assert.NotNull(service.CurrentIdentity.AuthKeyString);

        if (service.CurrentIdentity.AuthKeyString.Length == 0)
        {
            Assert.NotNull(service.CurrentIdentity.AuthKeyValue);
            Assert.NotEmpty(service.CurrentIdentity.AuthKeyValue);
        }
        else
        {
            Assert.NotEmpty(service.CurrentIdentity.AuthKeyString);
        }
    }
    
    [Fact]
    public void TestIdentityAuthenticationService_TrustedIdentities_Works()
    {
        var cfg = new Config();
        var service = new TestIdentityAuthenticationService(cfg);
        Assert.NotNull(service.TrustedIdentities);
        Assert.Empty(service.TrustedIdentities);
    }
    
    [Fact]
    public void TestIdentityAuthenticationService_UntrustedIdentities_Works()
    {
        var cfg = new Config();
        var service = new TestIdentityAuthenticationService(cfg);
        Assert.NotNull(service.UntrustedIdentities);
        Assert.Empty(service.UntrustedIdentities);
    }
    
    [Fact]
    public void TestIdentityAuthenticationService_SetEndpointForOriginGuid_Works()
    {
        var cfg = new Config();
        var service = new TestIdentityAuthenticationService(cfg);

        var originGuid = Guid.NewGuid();
        var actualEndpoint = service.GetEndpointForOriginGuid(originGuid, CancellationToken.None);
        
        Assert.Null(actualEndpoint);

        var expectedEndpoint = new Uri("http://example.com");

        service.SetEndpointForOriginGuid(originGuid, expectedEndpoint, CancellationToken.None);
        actualEndpoint = service.GetEndpointForOriginGuid(originGuid, CancellationToken.None);
        
        Assert.Equal(expectedEndpoint, actualEndpoint);
        
        service.SetEndpointForOriginGuid(originGuid, null, CancellationToken.None);
        actualEndpoint = service.GetEndpointForOriginGuid(originGuid, CancellationToken.None);
        
        Assert.Null(actualEndpoint);
    }
    
    [Fact]
    public void TestIdentityAuthenticationService_GetTrustLevel_Works()
    {
        var cfg = new Config();
        var service = new TestIdentityAuthenticationService(cfg);
        var identity = service.CurrentIdentity;
        var actualTrustLevel = service.GetTrustLevel(new IdentityModel(identity), CancellationToken.None);
        
        Assert.Equal(TrustLevel.Trusted, actualTrustLevel);
    }
    
    [Fact]
    public void TestIdentityAuthenticationService_SetTrustLevel_Works()
    {
        var cfg = new Config();
        var service = new TestIdentityAuthenticationService(cfg);

        var identity = new IdentityModel();
        var actualTrustLevel = service.GetTrustLevel(identity, CancellationToken.None);
        
        Assert.Equal(TrustLevel.Unknown, actualTrustLevel);
        
        service.SetTrustLevel(identity, TrustLevel.KnownButUntrusted, CancellationToken.None);
        actualTrustLevel = service.GetTrustLevel(identity, CancellationToken.None);
        
        Assert.Equal(TrustLevel.KnownButUntrusted, actualTrustLevel);
        
        service.SetTrustLevel(identity, TrustLevel.Trusted, CancellationToken.None);
        actualTrustLevel = service.GetTrustLevel(identity, CancellationToken.None);
        
        Assert.Equal(TrustLevel.Trusted, actualTrustLevel);
        
        service.SetTrustLevel(identity, TrustLevel.Unknown, CancellationToken.None);
        actualTrustLevel = service.GetTrustLevel(identity, CancellationToken.None);
        
        Assert.Equal(TrustLevel.Unknown, actualTrustLevel);
    }
}