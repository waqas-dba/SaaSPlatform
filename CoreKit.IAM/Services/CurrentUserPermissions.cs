using System.Security.Claims;
using CoreKit.IAM.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;

namespace CoreKit.IAM.Services;

public class CurrentUserPermissions : ICurrentUserPermissions
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;

    public CurrentUserPermissions(
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
    }

    public bool HasPermission(string permission)
    {
        var permissions = GetPermissions();
        return permissions.Contains(permission);
    }

    public IReadOnlyList<string> GetPermissions()
    {
        var userId = _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Array.Empty<string>();

        var cacheKey = $"permissions:{userId}";

        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            return LoadPermissionsFromClaims();
        })!;
    }

    private List<string> LoadPermissionsFromClaims()
    {
        var claims = _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        return claims ?? new List<string>();
    }
}