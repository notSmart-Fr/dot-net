using System.Security.Claims;
using StackExchange.Redis;

namespace TaskApi.Features.Auth;

public sealed class TokenRevocationService(IConnectionMultiplexer redis)
{
    private const string RevokedKeyPrefix = "auth:revoked-session:";

    public static string GetRevokedKey(string id) => $"{RevokedKeyPrefix}{id}";

    public static string? ExtractIdentifier(ClaimsPrincipal principal)
    {
        // Supabase uses 'session_id' for token sessions, falling back to 'sub' (User ID)
        return principal.FindFirst("session_id")?.Value 
            ?? principal.FindFirst("sub")?.Value 
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task RevokeAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var id = ExtractIdentifier(principal);
        if (string.IsNullOrWhiteSpace(id)) return;

        // Extract remaining lifetime from 'exp' Unix timestamp
        var ttl = TimeSpan.FromHours(1); // Default safety margin
        var expClaim = principal.FindFirst("exp")?.Value;
        if (long.TryParse(expClaim, out var expUnix))
        {
            var remaining = expUnix - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (remaining > 0)
            {
                ttl = TimeSpan.FromSeconds(remaining);
            }
        }

        var db = redis.GetDatabase();
        await db.StringSetAsync(GetRevokedKey(id), "1", ttl);
    }

    public async Task<bool> IsRevokedAsync(string? id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;

        var db = redis.GetDatabase();
        return await db.KeyExistsAsync(GetRevokedKey(id));
    }
}