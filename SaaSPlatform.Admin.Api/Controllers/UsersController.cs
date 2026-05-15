using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userService;
    public UsersController(IUserManagementService userService) => _userService = userService;

    [HttpPost]
    [RequiresPermission(Permissions.Users.Create)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(
            request.Name, request.Phone, request.Email, request.Password, request.TenantId);
        return Ok(new { user.Id, user.Name, user.Phone });
    }

    [HttpPost("{userId}/roles")]
    [RequiresPermission(Permissions.Users.AssignRole)]
    public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignRoleWithTenantRequest request)
    {
        await _userService.AssignRoleAsync(userId, request.RoleId, request.TenantId);
        return NoContent();
    }
}

public class CreateUserRequest
{
    public string Name { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public string Password { get; set; } = default!;
    public Guid? TenantId { get; set; }
}