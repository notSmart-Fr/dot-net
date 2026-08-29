using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using TaskApi.Infrastructure.Auth;

namespace TaskApi.Tests.Auth;

public class TokenRevocationServiceTests
{
    [Fact]
    public void GetRevokedJtiKey_UsesStablePrefix()
    {
        TokenRevocationService.GetRevokedJtiKey("abc123")
            .Should().Be("auth:revoked-jti:abc123");
    }

    [Fact]
    public void GetRemainingLifetime_UsesExpClaim_WhenPresent()
    {
        var exp = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Exp, exp.ToString())
        ]));

        var remaining = TokenRevocationService.GetRemainingLifetime(principal);

        remaining.Should().BeGreaterThan(TimeSpan.Zero);
        remaining.Should().BeLessThanOrEqualTo(TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(5)));
    }
}
