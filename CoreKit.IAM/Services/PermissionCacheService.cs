// CoreKit.IAM/Services/PermissionCacheService.cs
using System.Collections.Concurrent;
using CoreKit.IAM.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CoreKit.IAM.Services;

public class PermissionCacheService : IPermissionCacheService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    // userId -> set of cache keys owned by that user
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

        // Use a local copy of userId so the closure doesn't capture
        // a mutable struct from the outer scope.
        var capturedUserId = userId;
        var capturedKey = key;

        options.RegisterPostEvictionCallback((_, _, _, _) =>
        {
            if (!_userKeys.TryGetValue(capturedUserId, out var set)) return;
            set.TryRemove(capturedKey, out _);
            if (set.IsEmpty)
                _userKeys.TryRemove(capturedUserId, out _);
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

    // Stable, unambiguous key — does not embed colons inside variable segments
    private static string CacheKey(Guid userId, Guid? tenantId)
        => $"perm|{userId:N}|{(tenantId.HasValue ? tenantId.Value.ToString("N") : "global")}";
}