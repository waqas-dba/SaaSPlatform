// SaaSPlatform.Tenant.Api/Controllers/ProductAddonsController.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Persistence;
using CoreKit.IAM.Authorization;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/products/{productId}/addons")]
[Authorize]
public class ProductAddonsController : ApiControllerBase
{
    private readonly CatalogDbContext _db;

    public ProductAddonsController(CatalogDbContext db) => _db = db;

    [HttpGet]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetAddons(Guid productId)
    {
        var addons = await _db.Addons.Where(a => a.ProductId == productId).ToListAsync();
        return Ok(addons);
    }

    [HttpPost]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> Create(Guid productId, [FromBody] CreateAddonRequest request)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product is null) return NotFound();

        var addon = new Addon
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            AdditionalPrice = request.AdditionalPrice,
            AddonGroupId = Guid.Empty, // ad-hoc, not in group
            ProductId = productId
        };
        _db.Addons.Add(addon);
        await _db.SaveChangesAsync();
        return CreatedResponse(addon);
    }

    [HttpDelete("{addonId}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> Delete(Guid addonId)
    {
        var addon = await _db.Addons.FindAsync(addonId);
        if (addon is null) return NotFound();
        _db.Addons.Remove(addon);
        await _db.SaveChangesAsync();
        return DeletedResponse();
    }
}

public class CreateAddonRequest
{
    public string Name { get; set; } = default!;
    public decimal AdditionalPrice { get; set; }
}