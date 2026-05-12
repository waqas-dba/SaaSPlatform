using AuthCoreKit.IAM.Constants;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AuthCoreKit.IAM.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IamOptions _options;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IamOptions options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimConstants.UserId)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            if (_options.TenantResolver != null)
                return _options.TenantResolver(_httpContextAccessor.HttpContext!);
            return null;
        }
    }

    public bool IsAuthenticated => UserId.HasValue;

    public bool IsSuperAdmin =>
        _httpContextAccessor.HttpContext?.User
            .IsInRole(_options.SuperAdminRoleName) == true;

    public bool HasPermission(string permission)
        => _httpContextAccessor.HttpContext?.User
            .FindFirst(c => c.Type == ClaimConstants.Permission && c.Value == permission) != null;
}