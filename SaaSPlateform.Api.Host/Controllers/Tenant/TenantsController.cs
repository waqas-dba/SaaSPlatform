// SaaSPlatform.Api.Host/Controllers/Tenant/TenantsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.Infrastructure.Persistence;

namespace SaaSPlatform.Api.Host.Controllers.Tenant;

[ApiController]
[Route("api/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ITenantRegistrationService _registrationService;
    private readonly SaaSPlatformDbContext _db;

    public TenantsController(
        ITenantRegistrationService registrationService,
        SaaSPlatformDbContext db)
    {
        _registrationService = registrationService;
        _db = db;
    }
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(TenantRegistrationRequest request)
    {
        var (tenantId, message) = await _registrationService.RegisterAsync(
            request.RestaurantName,
            request.CuisineIds,
            request.ZoneIds,
            request.Address,
            request.MinPreparingTime,
            request.MaxPreparingTime,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Email,
            request.Password,
            request.CnicNumber,
            request.NtnNumber,
            request.HasFoodLicense,
            request.CnicFrontImageUrl,
            request.CnicBackImageUrl);

        return Ok(new TenantRegistrationResponse
        {
            TenantId = tenantId,
            Message = message
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetTenantInfo()
    {
        var tenantId = GetTenantId();
        if (tenantId is null) return Unauthorized();

        var tenant = await _db.Tenants.FindAsync(tenantId.Value);
        if (tenant is null) return NotFound();

        return Ok(new { tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive });
    }

    private Guid? GetTenantId()
    {
        return HttpContext.Items["TenantId"] as Guid?;
    }
}