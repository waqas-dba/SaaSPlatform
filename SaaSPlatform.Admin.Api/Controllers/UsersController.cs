using Asp.Versioning;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin/users")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
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
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await _userService.GetAllUsersAsync();
        return OkResponse(users);
    }

    [HttpGet("{userId:guid}")]
    [RequiresPermission(Permissions.Platform.ViewAllUsers)]
    public async Task<IActionResult> GetById(Guid userId, CancellationToken ct)
    {
        // Platform users have TenantId = null
        var user = await _userService.GetUserByIdAsync(userId, tenantId: null);
        return user is null ? NotFound() : OkResponse(user);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Platform.ManageAnyUser)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        var user = await _userService.CreateUserAsync(
            request.Name,
            request.Phone,
            request.Email,
            request.Password,
            request.TenantId);

        return CreatedResponse(new { user.Id, user.Name, user.Phone }, "User created successfully.");
    }

    [HttpPut("{userId:guid}")]
    [RequiresPermission(Permissions.Platform.ManageAnyUser)]
    public async Task<IActionResult> Update(
        Guid userId,
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        await _userService.UpdateUserAsync(
            userId,
            request.Name,
            request.Email,
            request.Phone,
            request.TenantId);
        return UpdatedResponse();
    }

    [HttpDelete("{userId:guid}")]
    [RequiresPermission(Permissions.Platform.ManageAnyUser)]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken ct)
    {
        await _userService.DeleteUserAsync(userId, tenantId: null, ct);
        return DeletedResponse();
    }

    [HttpPost("{userId}/roles")]
    [RequiresPermission(Permissions.Users.AssignRole)]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        [FromBody] AssignRoleWithTenantRequest request,
        CancellationToken ct)
    {
        var isPlatformAdmin = _currentUser.HasPermission(Permissions.Platform.ManageAnyUser);
        await _userService.AssignRoleAsync(
            userId,
            request.RoleId,
            request.TenantId,
            callerIsPlatformAdmin: isPlatformAdmin);
        return UpdatedResponse("Role assigned successfully.");
    }

    [HttpDelete("{userId}/roles/{roleId:guid}")]
    [RequiresPermission(Permissions.Users.RemoveRole)]
    public async Task<IActionResult> RemoveRole(
        Guid userId,
        Guid roleId,
        [FromQuery] Guid? tenantId,
        CancellationToken ct)
    {
        await _userService.RemoveRoleAsync(userId, roleId, tenantId);
        return DeletedResponse("Role removed.");
    }
}