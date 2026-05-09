using Microsoft.AspNetCore.Http;
using SaaSPlatform.Core.IAM.Constants;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Core.Tenant.Constants;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimConstants.UserId)?
                .Value;

            return Guid.TryParse(value, out var id)
                ? id
                : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;

            // PRIORITY:
            // USE MIDDLEWARE TENANT
            if (context?.Items[TenantConstants.TenantContextKey] is Guid tenantId)
                return tenantId;

            // FALLBACK TO JWT
            var value = context?
                .User
                .FindFirst(ClaimConstants.TenantId)?
                .Value;

            return Guid.TryParse(value, out var id)
                ? id
                : null;
        }
    }

    public bool IsAuthenticated =>
        UserId.HasValue;

    public bool IsSuperAdmin =>
        _httpContextAccessor.HttpContext?
            .User
            .IsInRole("SuperAdmin") == true;
}