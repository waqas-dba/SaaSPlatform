using CoreKit.IAM.Constants;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CoreKit.IAM.Services;

internal sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
        => _accessor = accessor;

    public Guid? UserId
    {
        get
        {
            var value = _accessor.HttpContext?
                .User
                .FindFirst(ClaimConstants.UserId)?
                .Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}