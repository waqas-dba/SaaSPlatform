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

    public UsersController(IUserManagementService userService)
        => _userService = userService;

    [HttpPost]
    [RequiresPermission(Permissions.Users.Create)]
    public async Task<IActionResult> Create(
        [FromBody] SaaSPlatform.Admin.Api.Models.CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(
            request.Name, request.Phone,
            request.Email, request.Password,
            request.TenantId);

        return CreatedResponse(new { user.Id, user.Name, user.Phone });
    }

    [HttpPost("{userId}/roles")]
    [RequiresPermission(Permissions.Users.AssignRole)]
    public async Task<IActionResult> AssignRole(
        Guid userId, [FromBody] AssignRoleWithTenantRequest request)
    {
        await _userService.AssignRoleAsync(
            userId, request.RoleId, request.TenantId);

        return UpdatedResponse("Role assigned successfully.");
    }
}