using System.Collections.Concurrent;
using CoreKit.IAM.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CoreKit.IAM.Services;

public class PermissionCacheService : IPermissionCacheService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    // Tracks all cache keys per user so InvalidateUserAsync can clear them all.
    // Uses ConcurrentDictionary<userId, ConcurrentDictionary<key, byte>> so
    // individual key removal is O(1) and the eviction callback can clean up correctly.
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _userKeys
        = new();

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

        // Register key in the per-user tracking dictionary
        var keySet = _userKeys.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
        keySet.TryAdd(key, 0);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Ttl
        };

        // Remove the key from the tracking set when it expires or is evicted
        // This prevents the memory leak from unbounded key accumulation
        options.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            if (evictedKey is string k &&
                _userKeys.TryGetValue(userId, out var set))
            {
                set.TryRemove(k, out _);

                // If the user has no more tracked keys, remove the user entry entirely
                if (set.IsEmpty)
                    _userKeys.TryRemove(userId, out _);
            }
        });

        _cache.Set(key, permissions, options);
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(Guid userId, Guid? tenantId)
    {
        var key = CacheKey(userId, tenantId);
        _cache.Remove(key);

        // Clean up tracking entry — the eviction callback does this too,
        // but doing it here ensures immediate consistency
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