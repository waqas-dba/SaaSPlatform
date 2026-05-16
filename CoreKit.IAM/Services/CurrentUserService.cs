// CoreKit.IAM | CoreKit.IAM/Services/CurrentUserService.cs
using System.Security.Claims;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Services;

public class CurrentUserService : ICurrentUserService, ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IamOptions _options;
    private readonly IamDbContext _db;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IamOptions options,
        IamDbContext db)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
        _db = db;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirst(ClaimConstants.UserId)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => UserId.HasValue;

    public bool IsSuperAdmin =>
        User?.IsInRole(_options.SuperAdminRoleName) == true;

    public bool HasPermission(string permission) =>
        User?.Claims.Any(c =>
            c.Type == ClaimConstants.Permission &&
            c.Value == permission) == true;

    public TenantScope GetTenantScope()
    {
        // FIX: do not throw for background-job / unauthenticated callers.
        // Return Global scope when there is no HTTP context so internal
        // services (cron jobs, seeders) don't crash. Callers that genuinely
        // require a tenant should use [RequiresPermission] or check
        // IsAuthenticated themselves.
        if (!IsAuthenticated) return TenantScope.Global;

        if (IsSuperAdmin) return TenantScope.Global;

        var claimValue = User?.FindFirst(ClaimConstants.TenantId)?.Value;
        if (Guid.TryParse(claimValue, out var tenantId) && tenantId != Guid.Empty)
            return TenantScope.For(tenantId);

        if (_options.TenantResolver != null)
        {
            var resolved = _options.TenantResolver(
                _httpContextAccessor.HttpContext!);
            if (resolved.HasValue)
                return TenantScope.For(resolved.Value);
        }

        // FIX: return Global rather than throwing — let the endpoint/filter
        // decide whether a missing tenant is an error for its own context.
        return TenantScope.Global;
    }

    public async Task<StoreScope> GetStoreScopeAsync()
    {
        if (!IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        if (IsSuperAdmin || HasPermission(Permissions.Store.ViewAll))
            return StoreScope.All;

        if (UserId == null)
            throw new UnauthorizedAccessException(
                "Cannot resolve store scope without a userId.");

        // FIX: read store IDs from JWT claims first — O(1), no DB round-trip.
        // The login flow already embeds storeIds into the token.
        var claimStoreIds = User!.Claims
            .Where(c => c.Type == ClaimConstants.StoreId)
            .Select(c => Guid.TryParse(c.Value, out var g) ? g : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();

        if (claimStoreIds.Count > 0)
            return StoreScope.For(claimStoreIds);

        // Fallback: DB query when claim is absent (e.g. older token, test client)
        var tenantScope = GetTenantScope();
        if (tenantScope.IsGlobal)
            return StoreScope.All;

        var dbStoreIds = await _db.Set<UserStoreAssignment>()
            .Where(a => a.UserId == UserId.Value &&
                        a.TenantId == tenantScope.TenantId)
            .Select(a => a.StoreId)
            .ToListAsync();

        if (dbStoreIds.Count == 0)
            throw new UnauthorizedAccessException(
                "User has no store assignments in this tenant.");

        return StoreScope.For(dbStoreIds);
    }
}