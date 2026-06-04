using Asp.Versioning;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class TenantUsersController : TenantApiControllerBase
{
    private readonly IUserManagementService _userService;
    private readonly IRoleManagementService _roleService;

    public TenantUsersController(
        IUserManagementService userService,
        IRoleManagementService roleService,
        ITenantContext tenantContext) : base(tenantContext)
    {
        _userService = userService;
        _roleService = roleService;
    }

    [HttpGet]
    [RequiresPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var users = await _userService.GetUsersAsync(tenantId);
        return OkResponse(users.Select(u => new { u.Id, u.Name, u.Email, u.Phone, u.IsActive }));
    }

    [HttpPost]
    [RequiresPermission(Permissions.Users.Create)]
    public async Task<IActionResult> Create([FromBody] CreateTenantUserRequest request, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var user = await _userService.CreateUserAsync(request.Name, request.Phone, request.Email, request.Password, tenantId);
        return CreatedResponse(new { user.Id, user.Name, user.Phone });
    }

    [HttpPut("{userId:guid}")]
    [RequiresPermission(Permissions.Users.Update)]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateTenantUserRequest request, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        await _userService.UpdateUserAsync(userId, request.Name, request.Email, request.Phone, tenantId);
        return UpdatedResponse();
    }

    [HttpDelete("{userId:guid}")]
    [RequiresPermission(Permissions.Users.Delete)]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        await _userService.DeleteUserAsync(userId, tenantId, ct);
        return DeletedResponse();
    }

    [HttpPost("{userId:guid}/roles")]
    [RequiresPermission(Permissions.Users.AssignRole)]
    public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignTenantRoleRequest request, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        await _userService.AssignRoleAsync(userId, request.RoleId, tenantId, callerIsPlatformAdmin: false);
        return UpdatedResponse("Role assigned.");
    }

    [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
    [RequiresPermission(Permissions.Users.RemoveRole)]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        await _userService.RemoveRoleAsync(userId, roleId, tenantId);
        return DeletedResponse("Role removed.");
    }

    [HttpGet("roles")]
    [RequiresPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var roles = await _roleService.GetRolesAsync(tenantId);
        return OkResponse(roles.Select(r => new { r.Id, r.Name, r.Description }));
    }
}