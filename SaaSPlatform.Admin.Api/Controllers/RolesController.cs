using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Admin.Api.Models;

namespace SaaSPlatform.Admin.Api.Controllers;

[Route("api/admin/roles")]
[Authorize]
public class RolesController : ApiControllerBase
{
    private readonly IRoleManagementService _roleService;

    public RolesController(IRoleManagementService roleService)
        => _roleService = roleService;

    [HttpPost]
    [RequiresPermission(Permissions.Roles.Create)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoleRequest request)
    {
        var role = await _roleService.CreateRoleAsync(
            request.Name, request.TenantId, request.Description);

        return CreatedResponse(new { role.Id, role.Name });
    }

    [HttpPost("{roleId}/permissions")]
    [RequiresPermission(Permissions.Roles.AssignPermission)]
    public async Task<IActionResult> AssignPermission(
        Guid roleId, [FromBody] AssignPermissionRequest request)
    {
        await _roleService.AssignPermissionAsync(
            roleId, request.PermissionId);

        return UpdatedResponse("Permission assigned successfully.");
    }
}