using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Public endpoint for tenant self‑registration.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]   // Removed [Authorize] – registration is public
    public async Task<IActionResult> Register(TenantRegistrationRequest request)
    {
        var result = await _tenantService.RegisterAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Admin‑only endpoint to approve a pending tenant.
    /// </summary>
    [HttpPost("{tenantId}/approve")]
    [Authorize]
    [RequiresPermission(Permissions.Tenants.Approve)]
    public async Task<IActionResult> Approve(Guid tenantId)
    {
        await _tenantService.ApproveAsync(tenantId);
        return Ok(new
        {
            tenantId = tenantId,
            status = "Approved",
            message = "Tenant approved successfully"
        });
    }

    [HttpGet]
    [Authorize]
    [RequiresPermission(Permissions.Tenants.View)]
    public async Task<IActionResult> GetAll()
    {
        var tenants = await _tenantService.GetAllAsync();
        return Ok(tenants.Select(t => new { t.Id, t.Name, t.Status }));
    }
}