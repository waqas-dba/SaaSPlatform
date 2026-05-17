using Microsoft.AspNetCore.Mvc;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Services;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using Microsoft.AspNetCore.Authorization;

namespace SaaSPlatform.Tenant.Api.Controllers;

/// <summary>
/// Store management endpoints, scoped to the current tenant.
/// </summary>
[ApiController]
[Route("api/stores")]
[Authorize]
public class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly ITenantContext _tenantContext;

    public StoresController(IStoreService storeService, ITenantContext tenantContext)
    {
        _storeService = storeService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [RequiresPermission(Permissions.Store.ViewAll)]  // SA uses ViewAll; tenant users use View
    public async Task<IActionResult> GetAll()
    {
        // SuperAdmin with no x-tenant-id header → list ALL stores across all tenants.
        // Tenant user must always provide x-tenant-id.
        if (_tenantContext.TenantId == null)
        {
            // Only SuperAdmin reaches here because RequiresPermission(Store.ViewAll)
            // blocks non-SA users without that permission, and tenant users always
            // have a tenantId in context from TenantResolutionMiddleware.
            var all = await _storeService.GetAllStoresAsync(); // see Gap 3 below
            return Ok(all);
        }

        var stores = await _storeService.GetAllByTenantAsync(_tenantContext.TenantId.Value);
        return Ok(stores);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> Create(CreateStoreRequest request)
    {
        if (_tenantContext.TenantId == null)
            return Unauthorized("Missing tenant context");

        var store = await _storeService.CreateAsync(request);
        return Ok(store);
    }

    [HttpPut("{storeId}")]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> Update(Guid storeId, UpdateStoreRequest request)
    {
        await _storeService.UpdateAsync(storeId, request);
        return NoContent();
    }

    [HttpDelete("{storeId}")]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> Delete(Guid storeId)
    {
        await _storeService.DeleteAsync(storeId);
        return NoContent();
    }

    [HttpPut("{storeId}/marketplace")]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> SetMarketplaceListing(Guid storeId, bool isListed)
    {
        await _storeService.SetMarketplaceListingAsync(storeId, isListed);
        return NoContent();
    }


    // Add a scoped View endpoint for tenant users:
    [HttpGet("my")]
    [RequiresPermission(Permissions.Store.View)]
    public async Task<IActionResult> GetMine()
    {
        if (_tenantContext.TenantId == null)
            return Unauthorized("Missing tenant context.");

        var stores = await _storeService.GetAllByTenantAsync(_tenantContext.TenantId.Value);
        return Ok(stores);
    }


}