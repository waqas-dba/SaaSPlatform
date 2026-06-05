using Asp.Versioning;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/stores")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class StoresController : TenantApiControllerBase
{
    private readonly IStoreService _storeService;

    public StoresController(
        IStoreService storeService,
        ITenantContext tenantContext) : base(tenantContext)
    {
        _storeService = storeService;
    }

    [HttpGet]
    [RequiresPermission(Permissions.Store.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var stores = await _storeService.GetAllByTenantAsync(tenantId, ct);
        return OkResponse(stores);
    }

    [HttpPost]
    [EnableRateLimiting("store-create")]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> Create(CreateStoreRequest request, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var store = await _storeService.CreateAsync(request, ct);
        return CreatedResponse(store);
    }

    [HttpPut("{storeId:guid}")]
    [EnableRateLimiting("store-update")]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> Update(Guid storeId, UpdateStoreRequest request, CancellationToken ct)
    {
        await _storeService.UpdateAsync(storeId, request, ct);
        return UpdatedResponse();
    }

    [HttpDelete("{storeId:guid}")]
    [EnableRateLimiting("store-update")]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> Delete(Guid storeId, CancellationToken ct)
    {
        await _storeService.DeleteAsync(storeId, ct);
        return DeletedResponse();
    }

    [HttpPut("{storeId:guid}/marketplace")]
    [EnableRateLimiting("store-update")]
    [RequiresPermission(Permissions.Store.Update)]
    public async Task<IActionResult> SetMarketplaceListing(Guid storeId, bool isListed, CancellationToken ct)
    {
        await _storeService.SetMarketplaceListingAsync(storeId, isListed, ct);
        return UpdatedResponse();
    }

    [HttpGet("my")]
    [RequiresPermission(Permissions.Store.View)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var stores = await _storeService.GetAllByTenantAsync(tenantId, ct);
        return OkResponse(stores);
    }

    // Add to SaaSPlatform.Tenant.Api/Controllers/StoresController.cs

    [HttpGet("{storeId:guid}")]
    [RequiresPermission(Permissions.Store.View)]
    public async Task<IActionResult> GetById(Guid storeId, CancellationToken ct)
    {
        var store = await _storeService.GetByIdAsync(storeId, ct);
        return store is null ? NotFound() : OkResponse(store);
    }
}