using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Admin.Api.Models;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v1/admin/roles")]
[Authorize]
public class RolesController : ApiControllerBase
{
    private readonly IRoleManagementService _roleService;

    public RolesController(IRoleManagementService roleService)
        => _roleService = roleService;

    [HttpGet]
    [RequiresPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var roles = await _roleService.GetRolesAsync(tenantId: null);
        return OkResponse(roles.Select(r => new { r.Id, r.Name, r.Description }));
    }

    [HttpGet("{roleId:guid}")]
    [RequiresPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetById(Guid roleId, CancellationToken ct)
    {
        var role = await _roleService.GetRoleByIdAsync(roleId, tenantId: null);
        return role is null ? NotFound() : OkResponse(new { role.Id, role.Name, role.Description });
    }

    [HttpPost]
    [RequiresPermission(Permissions.Roles.Create)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var role = await _roleService.CreateRoleAsync(request.Name, request.TenantId, request.Description);
        return CreatedResponse(new { role.Id, role.Name });
    }

    [HttpPut("{roleId:guid}")]
    [RequiresPermission(Permissions.Roles.Update)]
    public async Task<IActionResult> Update(Guid roleId, [FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        await _roleService.UpdateRoleAsync(roleId, request.Name, request.Description, request.TenantId);
        return UpdatedResponse();
    }

    [HttpDelete("{roleId:guid}")]
    [RequiresPermission(Permissions.Roles.Delete)]
    public async Task<IActionResult> Delete(Guid roleId, CancellationToken ct)
    {
        await _roleService.DeleteRoleAsync(roleId, tenantId: null);
        return DeletedResponse();
    }

    [HttpPost("{roleId}/permissions")]
    [RequiresPermission(Permissions.Roles.AssignPermission)]
    public async Task<IActionResult> AssignPermission(Guid roleId, [FromBody] AssignPermissionRequest request, CancellationToken ct)
    {
        await _roleService.AssignPermissionAsync(roleId, request.PermissionId);
        return UpdatedResponse("Permission assigned successfully.");
    }

    [HttpDelete("{roleId}/permissions/{permissionId:guid}")]
    [RequiresPermission(Permissions.Roles.RemovePermission)]
    public async Task<IActionResult> RemovePermission(Guid roleId, Guid permissionId, CancellationToken ct)
    {
        await _roleService.RemovePermissionAsync(roleId, permissionId);
        return DeletedResponse("Permission removed successfully.");
    }
}