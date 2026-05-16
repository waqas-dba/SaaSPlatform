// CoreKit.IAM | CoreKit.IAM/Services/PermissionService.cs
using CoreKit.IAM.Interfaces;

namespace CoreKit.IAM.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repo;
    private readonly IPermissionCacheService _cache;

    public PermissionService(
        IPermissionRepository repo,
        IPermissionCacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId, Guid? tenantId, string permission)
    {
        var permissions = await GetUserPermissionsAsync(userId, tenantId);
        return permissions.Contains(permission);
    }

    public async Task<bool> HasModuleAccessAsync(
        Guid userId, Guid? tenantId, string module)
    {
        var permissions = await GetUserPermissionsAsync(userId, tenantId);
        var prefix = module + ".";
        return permissions.Any(p => p.StartsWith(prefix));
    }

    public async Task<List<string>> GetUserPermissionsAsync(
        Guid userId, Guid? tenantId)
    {
        // FIX: serve from cache when available; only hit DB on miss.
        var cached = await _cache.GetAsync(userId, tenantId);
        if (cached != null) return cached;

        var fresh = await _repo.GetPermissionsAsync(userId, tenantId);
        await _cache.SetAsync(userId, tenantId, fresh);
        return fresh;
    }
}