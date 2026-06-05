// SaaSPlatform.Admin.Api/Controllers/AdminStoresController.cs
using Asp.Versioning;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin/stores")]
[Authorize]
[ApiVersion("1.0")]
public class AdminStoresController : ApiControllerBase
{
    private readonly IStoreService _storeService;

    public AdminStoresController(IStoreService storeService)
        => _storeService = storeService;

    [HttpGet]
    [RequiresPermission(Permissions.Platform.ViewAllStores)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var stores = await _storeService.GetAllStoresAsync(ct);
        return OkResponse(stores);
    }

    [HttpGet("tenant/{tenantId:guid}")]
    [RequiresPermission(Permissions.Platform.ViewAllStores)]
    public async Task<IActionResult> GetByTenant(Guid tenantId, CancellationToken ct)
    {
        var stores = await _storeService.GetAllByTenantAsync(tenantId, ct);
        return OkResponse(stores);
    }

    [HttpGet("{storeId:guid}")]
    [RequiresPermission(Permissions.Platform.ViewAllStores)]
    public async Task<IActionResult> GetById(Guid storeId, CancellationToken ct)
    {
        var store = await _storeService.GetByIdAsync(storeId, ct);
        return store is null ? NotFound() : OkResponse(store);
    }

    [HttpPut("{storeId:guid}")]
    [RequiresPermission(Permissions.Platform.ManageAnyStore)]
    public async Task<IActionResult> Update(
        Guid storeId, UpdateStoreRequest request, CancellationToken ct)
    {
        await _storeService.UpdateAsync(storeId, request, ct);
        return UpdatedResponse();
    }

    [HttpDelete("{storeId:guid}")]
    [RequiresPermission(Permissions.Platform.ManageAnyStore)]
    public async Task<IActionResult> Delete(Guid storeId, CancellationToken ct)
    {
        await _storeService.DeleteAsync(storeId, ct);
        return DeletedResponse();
    }
}