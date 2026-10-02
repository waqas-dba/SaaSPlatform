using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

/// <summary>
/// Which products a store sells, at what price, and whether they are available right now.
/// The products themselves belong to the tenant (see ProductsController).
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/stores/{storeId:guid}/menu")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class StoreMenuController : TenantApiControllerBase
{
    private readonly IStoreMenuService _menuService;

    public StoreMenuController(IStoreMenuService menuService, ITenantContext tenantContext)
        : base(tenantContext)
    {
        _menuService = menuService;
    }

    [HttpGet]
    [RequiresPermission(Permissions.Catalog.MenuView)]
    public async Task<IActionResult> GetMenu(
        Guid storeId, [FromQuery] ProductFilterQuery filter, CancellationToken ct)
    {
        var menu = await _menuService.GetMenuAsync(RequireTenantId(), storeId, filter, ct);
        return OkResponse(menu);
    }

    [HttpPost("products")]
    [RequiresPermission(Permissions.Catalog.MenuManage)]
    public async Task<IActionResult> AssignProducts(
        Guid storeId, AssignProductsRequest request, CancellationToken ct)
    {
        var added = await _menuService.AssignAsync(RequireTenantId(), storeId, request, ct);
        return OkResponse(new { Added = added });
    }

    [HttpPut("products/{productId:guid}")]
    [RequiresPermission(Permissions.Catalog.MenuManage)]
    public async Task<IActionResult> UpdateStoreProduct(
        Guid storeId, Guid productId, UpdateStoreProductRequest request, CancellationToken ct)
    {
        await _menuService.UpdateAsync(RequireTenantId(), storeId, productId, request, ct);
        return UpdatedResponse();
    }

    [HttpDelete("products/{productId:guid}")]
    [RequiresPermission(Permissions.Catalog.MenuManage)]
    public async Task<IActionResult> UnassignProduct(Guid storeId, Guid productId, CancellationToken ct)
    {
        await _menuService.UnassignAsync(RequireTenantId(), storeId, productId, ct);
        return DeletedResponse();
    }

    /// <summary>Copies another store's menu into this store.</summary>
    [HttpPost("copy")]
    [RequiresPermission(Permissions.Catalog.MenuManage)]
    public async Task<IActionResult> CopyMenu(Guid storeId, MenuTransferRequest request, CancellationToken ct)
    {
        var result = await _menuService.CopyAsync(RequireTenantId(), storeId, request, ct);
        return OkResponse(result);
    }

    /// <summary>Moves another store's menu into this store, leaving the source store's menu empty.</summary>
    [HttpPost("move")]
    [RequiresPermission(Permissions.Catalog.MenuManage)]
    public async Task<IActionResult> MoveMenu(Guid storeId, MenuTransferRequest request, CancellationToken ct)
    {
        var result = await _menuService.MoveAsync(RequireTenantId(), storeId, request, ct);
        return OkResponse(result);
    }
}