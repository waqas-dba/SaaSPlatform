using System.Collections.Concurrent;
using CoreKit.IAM.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CoreKit.IAM.Services;

public class PermissionCacheService : IPermissionCacheService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    // Tracks all cache keys that belong to a given userId so we can bulk-invalidate.
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _userKeys = new();

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

        var keySet = _userKeys.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
        keySet.TryAdd(key, 0);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Ttl
        };

        // BUG FIX: the original callback captured 'userId' from the outer scope correctly,
        // but also captured the live 'keySet' reference — if the keySet was removed from
        // _userKeys between Set and eviction, the callback would re-add a stale empty
        // keySet back via GetOrAdd semantics. Now we capture the key string only and
        // look up everything from _userKeys at eviction time to avoid stale state.
        options.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            if (evictedKey is not string k) return;

            // Parse userId out of the key instead of closing over it.
            // Key format: "perm:{userId}:{tenantId|global}"
            var parts = k.Split(':');
            if (parts.Length < 2 || !Guid.TryParse(parts[1], out var evictedUserId))
                return;

            if (!_userKeys.TryGetValue(evictedUserId, out var set)) return;

            set.TryRemove(k, out _);

            // Clean up the top-level entry when no keys remain for this user.
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

    // Key format: "perm:{userId}:{tenantId|global}"
    // The eviction callback depends on this format — keep them in sync.
    private static string CacheKey(Guid userId, Guid? tenantId)
        => $"perm:{userId}:{tenantId?.ToString() ?? "global"}";
}