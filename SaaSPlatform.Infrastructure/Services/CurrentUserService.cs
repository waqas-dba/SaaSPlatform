using Microsoft.AspNetCore.Http;
using SaaSPlatform.Core.IAM.Constants;

namespace SaaSPlatform.Infrastructure.Services;

public class CurrentUserService
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
            var value =
                _httpContextAccessor.HttpContext?
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
            var value =
                _httpContextAccessor.HttpContext?
                    .User
                    .FindFirst(ClaimConstants.TenantId)?
                    .Value;

            return Guid.TryParse(value, out var id)
                ? id
                : null;
        }
    }
}