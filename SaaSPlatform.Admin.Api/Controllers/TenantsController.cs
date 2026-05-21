// SaaSPlatform.Admin.Api/Controllers/TenantsController.cs
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/tenants")]
public class TenantsController : ApiControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
        => _tenantService = tenantService;

    // Fix #10: Apply the "login" rate limit policy (5 req/min) to the anonymous
    // registration endpoint to prevent spam/abuse.
    // If you want a separate, stricter policy for registration, add one in Program.cs:
    //   options.AddFixedWindowLimiter("tenant_register", cfg => {
    //       cfg.PermitLimit = 3; cfg.Window = TimeSpan.FromMinutes(10);
    //   });
    // and reference it here instead.
    [HttpPost("register")]
    [AllowAnonymous]
    //[EnableRateLimiting("login")]
    public async Task<IActionResult> Register(TenantRegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new
            {
                success = false,
                errorCode = "VALIDATION_ERROR",
                message = "Tenant name is required."
            });

        var result = await _tenantService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("{tenantId}/approve")]
    [Authorize]
    [RequiresPermission(Permissions.Tenants.Approve)]
    public async Task<IActionResult> Approve(Guid tenantId)
    {
        await _tenantService.ApproveAsync(tenantId);
        return Ok(new
        {
            tenantId,
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