using Asp.Versioning;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin/tenants")]
[Asp.Versioning.ApiVersion("1.0")]
public class TenantsController : ApiControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly IRoleManagementService _roleManagementService;
    private readonly IRoleRepository _roleRepository;

    public TenantsController(
        ITenantService tenantService,
        IRoleManagementService roleManagementService,
        IRoleRepository roleRepository)
    {
        _tenantService = tenantService;
        _roleManagementService = roleManagementService;
        _roleRepository = roleRepository;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(TenantRegistrationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { success = false, errorCode = "VALIDATION_ERROR", message = "Tenant name is required." });

        var result = await _tenantService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("{tenantId}/approve")]
    [Authorize]
    [RequiresPermission(Permissions.Tenants.Approve)]
    public async Task<IActionResult> Approve(Guid tenantId, CancellationToken ct)
    {
        await _tenantService.ApproveAsync(tenantId);
        await EnsureDefaultTenantRoleAsync(tenantId);
        return Ok(new { tenantId, status = "Approved", message = "Tenant approved successfully" });
    }

    [HttpGet]
    [Authorize]
    [RequiresPermission(Permissions.Tenants.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenants = await _tenantService.GetAllAsync();
        return OkResponse(tenants.Select(t => new { t.Id, t.Name, t.Status }));
    }

    private async Task EnsureDefaultTenantRoleAsync(Guid tenantId)
    {
        const string roleName = "TenantAdmin";
        var existingRoles = await _roleManagementService.GetRolesAsync(tenantId);
        if (existingRoles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
            return;

        var role = await _roleManagementService.CreateRoleAsync(roleName, tenantId, "Default tenant administrator – full store and catalog management");
        var allPermissions = await _roleRepository.GetAllPermissionsAsync();

        string[] requiredPermissions =
        {
            "store.view", "store.update",
            "catalog.categories.view", "catalog.categories.create", "catalog.categories.update", "catalog.categories.delete",
            "catalog.products.view", "catalog.products.create", "catalog.products.update", "catalog.products.delete",
            "catalog.templates.view", "catalog.templates.manage.tenant", "catalog.templates.assign", "catalog.templates.toggle.store"
        };

        foreach (var permName in requiredPermissions)
        {
            var perm = allPermissions.FirstOrDefault(p => p.Name == permName);
            if (perm != null)
                await _roleManagementService.AssignPermissionAsync(role.Id, perm.Id);
        }
    }
}