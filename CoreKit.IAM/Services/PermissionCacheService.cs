// CoreKit.IAM/Services/PermissionCacheService.cs

using System.Collections.Concurrent;
using CoreKit.IAM.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CoreKit.IAM.Services;

/// <summary>
/// In-process permission cache backed by <see cref="IMemoryCache"/>.
/// <para>
/// <b>Single-node deployments only.</b>  In a horizontally-scaled environment
/// every node maintains its own independent cache, so a role change on one
/// node will not immediately invalidate caches on other nodes.  For
/// multi-instance deployments replace this registration with a distributed
/// implementation (e.g. Redis via <c>IDistributedCache</c>) and register it
/// against <c>IPermissionCacheService</c> in <c>ServiceCollectionExtensions</c>.
/// </para>
/// </summary>
public class PermissionCacheService : IPermissionCacheService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    // Tracks every cache key per user so InvalidateUserAsync can sweep all
    // tenant-scoped entries without knowing the tenant IDs in advance.
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>>
        _userKeys = new();

    private readonly IMemoryCache _cache;

    public PermissionCacheService(IMemoryCache cache) => _cache = cache;

    public Task<List<string>?> GetAsync(Guid userId, Guid? tenantId)
    {
        _cache.TryGetValue(CacheKey(userId, tenantId), out List<string>? result);
        return Task.FromResult(result);
    }

    public Task SetAsync(Guid userId, Guid? tenantId, List<string> permissions)
    {
        var key = CacheKey(userId, tenantId);
        var keySet = _userKeys.GetOrAdd(
            userId, _ => new ConcurrentDictionary<string, byte>());
        keySet.TryAdd(key, 0);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Ttl
        };

        options.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            if (evictedKey is not string k) return;

            var parts = k.Split(':');
            if (parts.Length < 2 || !Guid.TryParse(parts[1], out var evictedUserId))
                return;

            if (!_userKeys.TryGetValue(evictedUserId, out var set)) return;
            set.TryRemove(k, out _);

            if (set.IsEmpty)
                _userKeys.TryRemove(evictedUserId, out _);
        });

        _cache.Set(key, permissions, options);
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(Guid userId, Guid? tenantId)
    {
        var key = CacheKey(userId, tenantId);
        _cache.Remove(key);

        if (_userKeys.TryGetValue(userId, out var keySet))
        {
            keySet.TryRemove(key, out _);
            if (keySet.IsEmpty)
                _userKeys.TryRemove(userId, out _);
        }

        return Task.CompletedTask;
    }

    public Task InvalidateUserAsync(Guid userId)
    {
        if (!_userKeys.TryRemove(userId, out var keySet))
            return Task.CompletedTask;

        foreach (var key in keySet.Keys)
            _cache.Remove(key);

        return Task.CompletedTask;
    }

    private static string CacheKey(Guid userId, Guid? tenantId)
        => $"perm:{userId}:{tenantId?.ToString() ?? "global"}";
}