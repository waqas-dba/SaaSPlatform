using System.Security.Claims;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CoreKit.IAM.Services;

public class CurrentUserService : ICurrentUserService, ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IamOptions _options;
    private readonly IUserStoreAssignmentRepository _storeAssignmentRepo;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IamOptions options,
        IUserStoreAssignmentRepository storeAssignmentRepo)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
        _storeAssignmentRepo = storeAssignmentRepo;
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

    public bool IsPlatformAdmin =>
        User?.IsInRole(_options.PlatformAdminRoleName) == true;

    public bool HasPermission(string permission) =>
        User?.Claims.Any(c =>
            c.Type == ClaimConstants.Permission &&
            c.Value == permission) == true;

    public TenantScope GetTenantScope()
    {
        if (!IsAuthenticated) return TenantScope.Global;

        // Platform admins with explicit permission get global scope
        if (HasPermission(Permissions.Platform.ViewAllTenants))
            return TenantScope.Global;

        // Check for tenant claim in JWT
        var claimValue = User?.FindFirst(ClaimConstants.TenantId)?.Value;
        if (Guid.TryParse(claimValue, out var tenantId) && tenantId != Guid.Empty)
            return TenantScope.For(tenantId);

        // Fallback to resolver
        if (_options.TenantResolver != null)
        {
            var resolved = _options.TenantResolver(_httpContextAccessor.HttpContext!);
            if (resolved.HasValue)
                return TenantScope.For(resolved.Value);
        }

        return TenantScope.Global;
    }

    public async Task<StoreScope> GetStoreScopeAsync()
    {
        if (!IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        // Platform permission for all stores
        if (HasPermission(Permissions.Store.ViewAll) ||
            HasPermission(Permissions.Platform.ViewAllStores))
            return StoreScope.All;

        if (UserId == null)
            throw new UnauthorizedAccessException("Cannot resolve store scope without a userId.");

        // Check store claims from JWT
        var claimStoreIds = User!.Claims
            .Where(c => c.Type == ClaimConstants.StoreId)
            .Select(c => Guid.TryParse(c.Value, out var g) ? g : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();

        if (claimStoreIds.Count > 0)
            return StoreScope.For(claimStoreIds);

        // Check tenant scope
        var tenantScope = GetTenantScope();
        if (tenantScope.IsGlobal)
            return StoreScope.All;

        // Fallback to database lookup
        var dbStoreIds = await _storeAssignmentRepo.GetStoreIdsByUserAsync(
            UserId.Value, tenantScope.TenantId);

        // Return empty scope instead of throwing when user has no store assignments
        if (dbStoreIds.Count == 0)
            return StoreScope.Empty;

        return StoreScope.For(dbStoreIds);
    }
}