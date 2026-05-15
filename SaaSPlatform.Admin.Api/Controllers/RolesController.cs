using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleManagementService _roleService;
    public RolesController(IRoleManagementService roleService) => _roleService = roleService;

    [HttpPost]
    [RequiresPermission(Permissions.Roles.Create)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var role = await _roleService.CreateRoleAsync(request.Name, request.TenantId, request.Description);
        return Ok(new { role.Id, role.Name });
    }

    [HttpPost("{roleId}/permissions")]
    [RequiresPermission(Permissions.Roles.AssignPermission)]
    public async Task<IActionResult> AssignPermission(Guid roleId, [FromBody] AssignPermissionRequest request)
    {
        await _roleService.AssignPermissionAsync(roleId, request.PermissionId);
        return NoContent();
    }
}

public class CreateRoleRequest
{
    public string Name { get; set; } = default!;
    public Guid? TenantId { get; set; }
    public string? Description { get; set; }
}

public class AssignPermissionRequest
{
    public Guid PermissionId { get; set; }
}