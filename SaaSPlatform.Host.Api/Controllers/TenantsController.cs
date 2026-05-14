using Microsoft.AspNetCore.Mvc;
using CoreKit.Contracts.Interfaces;
using CoreKit.Contracts.Models;

namespace SaaSPlatform.Host.Api.Controllers;

[ApiController]
[Route("api/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService) => _tenantService = tenantService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(TenantRegistrationRequest request)
    {
        var result = await _tenantService.RegisterAsync(request);
        if (!result.Succeeded)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpGet("{tenantId}")]
    public async Task<IActionResult> Get(Guid tenantId)
    {
        var tenant = await _tenantService.GetByIdAsync(tenantId);
        return tenant == null ? NotFound() : Ok(tenant);
    }
}