// SaaSPlatform.Admin.Api/Controllers/ProductAddonsController.cs
using CoreKit.Catalog.Interfaces;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/products/{productId}/addons")]
[Authorize]
public class ProductAddonsController : ApiControllerBase
{
    private readonly IAddonService _addonService;
    private readonly IProductService _productService;
    private readonly IStoreService _storeService;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantContext _tenantContext;

    public ProductAddonsController(
        IAddonService addonService,
        IProductService productService,
        IStoreService storeService,
        ICurrentUserService currentUser,
        ITenantContext tenantContext)
    {
        _addonService = addonService;
        _productService = productService;
        _storeService = storeService;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetAddons(Guid productId)
    {
        if (!await HasAccessToProductAsync(productId))
            return Forbid();

        var addons = await _addonService.GetByProductAsync(productId);
        return Ok(addons);
    }

    [HttpPost]
    [RequiresPermission("catalog.products.update")]
    [EnableRateLimiting("product-update")]
    public async Task<IActionResult> Create(
        Guid productId,
        [FromBody] CreateAddonRequest request)
    {
        if (!await HasAccessToProductAsync(productId))
            return Forbid();

        var addon = await _addonService.CreateAdHocAsync(
            productId, request.Name, request.AdditionalPrice);
        return CreatedResponse(addon);
    }

    [HttpDelete("{addonId}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> Delete(Guid productId, Guid addonId)
    {
        // Ensure the caller owns the product before modifying its addons
        if (!await HasAccessToProductAsync(productId))
            return Forbid();

        await _addonService.DeleteAsync(addonId);
        return DeletedResponse();
    }

    private async Task<bool> HasAccessToProductAsync(Guid productId)
    {
        // Platform users with cross‑store permissions bypass the tenant check.
        if (_currentUser.HasPermission(Permissions.Platform.ViewAllStores) ||
            _currentUser.HasPermission(Permissions.Store.ViewAll))
            return true;

        var product = await _productService.GetByIdAsync(productId);
        if (product == null) return false;

        // Tenant‑scoped users must own the product's store.
        var store = await _storeService.GetByIdAsync(product.StoreId);
        return store != null && store.TenantId == _tenantContext.TenantId;
    }
}

public class CreateAddonRequest
{
    public string Name { get; set; } = default!;
    public decimal AdditionalPrice { get; set; }
}