// SaaSPlatform.Admin.Api/Controllers/UsersController.cs

using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Admin.Api.Models;

namespace SaaSPlatform.Admin.Api.Controllers;

[Route("api/admin/users")]
[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly IUserManagementService _userService;
    private readonly ICurrentUserService _currentUser;

    public UsersController(
        IUserManagementService userService,
        ICurrentUserService currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [RequiresPermission(Permissions.Platform.ViewAllUsers)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Platform.ManageAnyUser)]
    public async Task<IActionResult> Create(
        [FromBody] SaaSPlatform.Admin.Api.Models.CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(
            request.Name,
            request.Phone,
            request.Email,
            request.Password,
            request.TenantId);

        return CreatedResponse(
            new { user.Id, user.Name, user.Phone },
            "Created successfully.");
    }

    [HttpPost("{userId}/roles")]
    [RequiresPermission(Permissions.Users.AssignRole)]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        [FromBody] AssignRoleWithTenantRequest request)
    {
        // Resolve the authorization flag here in the HTTP layer — this is the
        // only place that has access to the JWT claims. The service itself no
        // longer depends on ICurrentUserService so it stays testable.
        var isPlatformAdmin = _currentUser.HasPermission(
            Permissions.Platform.ManageAnyUser);

        await _userService.AssignRoleAsync(
            userId,
            request.RoleId,
            request.TenantId,
            callerIsPlatformAdmin: isPlatformAdmin);

        return UpdatedResponse("Role assigned successfully.");
    }
}