using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.Infrastructure.Persistence.Repositories;

namespace SaaSPlatform.Api.Host.Controllers.Tenant;

[ApiController]
[Route("api/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ITenantRegistrationService _registrationService;
    private readonly ITenantAccountRepository _tenantRepo;

    public TenantsController(
        ITenantRegistrationService registrationService,
        ITenantAccountRepository tenantRepo)
    {
        _registrationService = registrationService;
        _tenantRepo = tenantRepo;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(TenantRegistrationRequest request)
    {
        var (tenantId, message) = await _registrationService.RegisterAsync(
            request.RestaurantName, request.CuisineIds, request.ZoneIds, request.Address,
            request.MinPreparingTime, request.MaxPreparingTime,
            request.FirstName, request.LastName, request.Phone, request.Email,
            request.Password, request.CnicNumber, request.NtnNumber,
            request.HasFoodLicense, request.CnicFrontImageUrl, request.CnicBackImageUrl);

        return Ok(ApiResponse<TenantRegistrationResponse>.SuccessResponse(
            new TenantRegistrationResponse { TenantId = tenantId, Message = message }));
    }

    [HttpGet]
    public async Task<IActionResult> GetTenantInfo()
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return Unauthorized(ApiResponse<object>.FailResponse("Missing tenant context."));

        var tenant = await _tenantRepo.GetByIdAsync(tenantId.Value);
        if (tenant is null)
            return NotFound(ApiResponse<object>.FailResponse("Tenant not found."));

        return Ok(ApiResponse<object>.SuccessResponse(new { tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive }));
    }

    private Guid? GetTenantId() => HttpContext.Items["TenantId"] as Guid?;
}