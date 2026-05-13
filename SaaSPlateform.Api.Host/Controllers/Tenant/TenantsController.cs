using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;

namespace SaaSPlatform.Api.Host.Controllers.Tenant;

[ApiController]
[Route("api/tenants")]
public class TenantsController : BaseApiController
{
    private readonly ITenantRegistrationService _registration;
    private readonly ITenantAccountRepository _repo;

    public TenantsController(
        ITenantRegistrationService registration,
        ITenantAccountRepository repo)
    {
        _registration = registration;
        _repo = repo;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(TenantRegistrationRequest request)
    {
        var result = await _registration.RegisterAsync(
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
            return Fail(result.Message);

        return Success(new TenantRegistrationResponse
        {
            TenantId = result.Data,
            Message = result.Message
        });
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (TenantId is null)
            return Fail("Missing tenant context", status: 401);

        var tenant = await _repo.GetByIdAsync(TenantId.Value);

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