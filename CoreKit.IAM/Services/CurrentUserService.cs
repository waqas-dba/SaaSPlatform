using System.Security.Claims;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using Microsoft.AspNetCore.Http;

namespace CoreKit.IAM.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IamOptions _options;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IamOptions options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?
                .FindFirst(ClaimConstants.UserId)?.Value;

            return Guid.TryParse(value, out var id)
                ? id
                : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var claimValue = User?
                .FindFirst(ClaimConstants.TenantId)?.Value;

            if (Guid.TryParse(claimValue, out var tenantId))
            {
                return tenantId;
            }

            if (_options.TenantResolver != null)
            {
                return _options.TenantResolver(
                    _httpContextAccessor.HttpContext!);
            }

            return null;
        }
    }

    public bool IsAuthenticated => UserId.HasValue;

    public bool IsSuperAdmin =>
        User?.IsInRole(_options.SuperAdminRoleName) == true;

    public bool HasPermission(string permission)
    {
        return User?.Claims.Any(c =>
            c.Type == ClaimConstants.Permission &&
            c.Value == permission) == true;
    }
}