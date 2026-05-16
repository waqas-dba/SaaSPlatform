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

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirst(ClaimConstants.UserId)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => UserId.HasValue;
    public bool IsSuperAdmin => User?.IsInRole(_options.SuperAdminRoleName) == true;

    public bool HasPermission(string permission) =>
        User?.Claims.Any(c => c.Type == ClaimConstants.Permission && c.Value == permission) == true;

    public TenantScope GetTenantScope()
    {
        if (!IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        if (IsSuperAdmin) return TenantScope.Global;

        var claimValue = User?.FindFirst(ClaimConstants.TenantId)?.Value;
        if (Guid.TryParse(claimValue, out var tenantId) && tenantId != Guid.Empty)
            return TenantScope.For(tenantId);

        if (_options.TenantResolver != null)
        {
            var resolved = _options.TenantResolver(_httpContextAccessor.HttpContext!);
            if (resolved.HasValue) return TenantScope.For(resolved.Value);
        }

        throw new UnauthorizedAccessException("Tenant context is required but could not be resolved.");
    }

    public async Task<StoreScope> GetStoreScopeAsync()
    {
        if (!IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        if (IsSuperAdmin || HasPermission(Permissions.Store.ViewAll))
            return StoreScope.All;

        if (UserId == null)
            throw new UnauthorizedAccessException("Cannot resolve store scope without a userId.");

        var tenantScope = GetTenantScope();

        var storeIds = await _db.Set<UserStoreAssignment>()
            .Where(a => a.UserId == UserId.Value && a.TenantId == tenantScope.TenantId)
            .Select(a => a.StoreId)
            .ToListAsync();

        if (storeIds.Count == 0)
            throw new UnauthorizedAccessException("User has no store assignments in this tenant.");

        return StoreScope.For(storeIds);
    }
}