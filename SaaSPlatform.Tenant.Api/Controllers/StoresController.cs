using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Tenancy;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/stores")]
[Authorize]
public class StoresController : ApiControllerBase
{
    private readonly IStoreService _storeService;
    private readonly ITenantContext _tenantContext;

    public StoresController(
        IStoreService storeService,
        ITenantContext tenantContext)
    {
        _storeService = storeService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [RequiresPermission(Permissions.Store.View)]
    public async Task<IActionResult> GetAll()
    {
        if (_tenantContext.TenantId == null)
        {
            return Unauthorized(new
            {
                success = false,
                errorCode = "UNAUTHORIZED",
                message = "Missing tenant context."
            });
        }

        var stores =
            await _storeService.GetAllByTenantAsync(
                _tenantContext.TenantId.Value);

        return Ok(stores);
    }

    [HttpPost]
    [EnableRateLimiting("store-create")]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> Create(
        CreateStoreRequest request)
    {
        if (_tenantContext.TenantId == null)
        {
            return Unauthorized(new
            {
                success = false,
                errorCode = "UNAUTHORIZED",
                message = "Missing tenant context."
            });
        }

        var store = await _storeService.CreateAsync(request);

        return Ok(store);
    }

    [HttpPut("{storeId:guid}")]
    [EnableRateLimiting("store-update")]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> Update(
        Guid storeId,
        UpdateStoreRequest request)
    {
        await _storeService.UpdateAsync(storeId, request);

        return NoContent();
    }

    [HttpDelete("{storeId:guid}")]
    [EnableRateLimiting("store-update")]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> Delete(Guid storeId)
    {
        await _storeService.DeleteAsync(storeId);

        return NoContent();
    }

    [HttpPut("{storeId:guid}/marketplace")]
    [EnableRateLimiting("store-update")]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> SetMarketplaceListing(
        Guid storeId,
        bool isListed)
    {
        await _storeService.SetMarketplaceListingAsync(
            storeId,
            isListed);

        return NoContent();
    }

    [HttpGet("my")]
    [RequiresPermission(Permissions.Store.View)]
    public async Task<IActionResult> GetMine()
    {
        if (_tenantContext.TenantId == null)
        {
            return Unauthorized(new
            {
                success = false,
                errorCode = "UNAUTHORIZED",
                message = "Missing tenant context."
            });
        }

        var stores =
            await _storeService.GetAllByTenantAsync(
                _tenantContext.TenantId.Value);

        return Ok(stores);
    }
}