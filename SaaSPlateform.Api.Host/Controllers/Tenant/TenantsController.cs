using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Api.Host.Controllers;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;

[ApiController]
[Route("api/tenants")]
public class TenantsController : BaseApiController
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
        var result = await _registrationService.RegisterAsync(
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

        if (!result.Succeeded)
            return Fail(result.Message, status: 400);

        return Success(new TenantRegistrationResponse
        {
            TenantId = result.Data,
            Message = result.Message
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetTenantInfo()
    {
        if (TenantId is null)
            return Fail("Missing tenant context", status: 401);

        var tenant = await _tenantRepo.GetByIdAsync(TenantId.Value);
        if (tenant is null)
            return Fail("Tenant not found", status: 404);

        return Success(new
        {
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.IsActive
        });
    }
}