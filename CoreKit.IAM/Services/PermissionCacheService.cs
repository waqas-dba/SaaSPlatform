// CoreKit.IAM | CoreKit.IAM/Services/PermissionCacheService.cs
using CoreKit.IAM.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CoreKit.IAM.Services;

public class PermissionCacheService : IPermissionCacheService
{
    // Cache permissions for 5 minutes. Short enough that role changes
    // propagate quickly; long enough to absorb repeated DB hits per request.
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private readonly IMemoryCache _cache;

    public PermissionCacheService(IMemoryCache cache) => _cache = cache;

    public Task<List<string>?> GetAsync(Guid userId, Guid? tenantId)
    {
        _cache.TryGetValue(CacheKey(userId, tenantId), out List<string>? result);
        return Task.FromResult(result);
    }

    public Task SetAsync(Guid userId, Guid? tenantId, List<string> permissions)
    {
        _cache.Set(CacheKey(userId, tenantId), permissions,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl });
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(Guid userId, Guid? tenantId)
    {
        _cache.Remove(CacheKey(userId, tenantId));
        return Task.CompletedTask;
    }

    public Task InvalidateUserAsync(Guid userId)
    {
        // Remove global + all known tenant slots.
        // For a distributed scenario replace this with Redis SCAN on the prefix.
        _cache.Remove(CacheKey(userId, null));
        return Task.CompletedTask;
    }

    private static string CacheKey(Guid userId, Guid? tenantId)
        => $"perm:{userId}:{tenantId?.ToString() ?? "global"}";
}