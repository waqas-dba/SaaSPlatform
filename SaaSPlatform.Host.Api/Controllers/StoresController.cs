using Microsoft.AspNetCore.Mvc;
using CoreKit.Contracts.Interfaces;
using CoreKit.Contracts.Models;
using CoreKit.Tenant.Abstractions;

namespace SaaSPlatform.Host.Api.Controllers;

[ApiController]
[Route("api/stores")]
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
    public async Task<IActionResult> GetAll()
    {
        if (_tenantContext.TenantId == null) return Unauthorized("Missing tenant context");
        var stores = await _storeService.GetAllByTenantAsync(_tenantContext.TenantId.Value);
        return Ok(stores);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UpdateStoreRequest request)
    {
        if (_tenantContext.TenantId == null) return Unauthorized("Missing tenant context");
        var store = await _storeService.CreateAsync(_tenantContext.TenantId.Value, request.Name!, request.Type, request.MetadataJson);
        return Ok(store);
    }

    [HttpPut("{storeId}")]
    public async Task<IActionResult> Update(Guid storeId, UpdateStoreRequest request)
    {
        await _storeService.UpdateAsync(storeId, request);
        return NoContent();
    }

    [HttpDelete("{storeId}")]
    public async Task<IActionResult> Delete(Guid storeId)
    {
        await _storeService.DeleteAsync(storeId);
        return NoContent();
    }

    // Admin-only marketplace toggle (secured by IAM’s [RequiresPermission] attribute)
    [HttpPut("{storeId}/marketplace")]
    public async Task<IActionResult> SetMarketplaceListing(Guid storeId, bool isListed)
    {
        await _storeService.SetMarketplaceListingAsync(storeId, isListed);
        return NoContent();
    }
}