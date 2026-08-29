using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using StackExchange.Redis;

namespace TaskApi.Infrastructure.Auth;

public sealed class TokenRevocationService(IConnectionMultiplexer redis)
{
    private const string RevokedJtiPrefix = "auth:revoked-jti:";

    public static string GetRevokedJtiKey(string jti) => $"{RevokedJtiPrefix}{jti}";

    public static TimeSpan GetRemainingLifetime(ClaimsPrincipal principal)
    {
        var expClaim = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value
            ?? principal.FindFirst("exp")?.Value;

        if (string.IsNullOrWhiteSpace(expClaim))
        {
            return TimeSpan.Zero;
        }

        if (!long.TryParse(expClaim, out var expUnix))
        {
            return TimeSpan.Zero;
        }

        var remainingSeconds = expUnix - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return remainingSeconds > 0 ? TimeSpan.FromSeconds(remainingSeconds) : TimeSpan.Zero;
    }

    public async Task RevokeAsync(string jti, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return;
        }

        var ttl = GetRemainingLifetime(principal);
        if (ttl <= TimeSpan.Zero)
        {
            return;
        }

        var db = redis.GetDatabase();
        await db.StringSetAsync(GetRevokedJtiKey(jti), "1", ttl, When.Always, CommandFlags.None);
    }

    public async Task<bool> IsRevokedAsync(string? jti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return false;
        }

        var db = redis.GetDatabase();
        return await db.KeyExistsAsync(GetRevokedJtiKey(jti));
    }
}
