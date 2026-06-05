// SaaSPlatform.Public.Api/Controllers/ProfileController.cs
using Asp.Versioning;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Public.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/me")]
[Authorize]
[ApiVersion("1.0")]
public class ProfileController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserManagementService _userService;

    public ProfileController(
        ICurrentUserService currentUser,
        IUserManagementService userService)
    {
        _currentUser = currentUser;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Not authenticated.");

        var scope = _currentUser.GetTenantScope();
        var tenantId = scope.IsGlobal ? (Guid?)null : scope.TenantId;

        var user = await _userService.GetUserByIdAsync(userId, tenantId)
            ?? throw new KeyNotFoundException("User not found.");

        return OkResponse(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            user.IsActive,
            user.LastLoginAt
        });
    }
}