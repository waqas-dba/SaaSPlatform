using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantKit.Interfaces;
using TenantKit.Models;

namespace SaaSPlatform.Api.Host.Controllers.Tenant;

[ApiController]
[Route("api/tenants")]
public class TenantsController : BaseApiController
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService) => _tenantService = tenantService;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(TenantRegistrationRequest request)
    {
        var response = await _tenantService.RegisterAsync(request);
        return Success(response);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (TenantId is null) return Fail("Missing tenant context", status: 401);
        var tenant = await _tenantService.GetByIdAsync(TenantId.Value);
        if (tenant is null) return Fail("Tenant not found", status: 404);
        return Success(new { tenant.Id, tenant.Name, tenant.Slug, tenant.Status });
    }
}