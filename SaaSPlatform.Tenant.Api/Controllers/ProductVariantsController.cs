using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/products/{productId}/variants")]
[Authorize]
public class ProductVariantsController : ApiControllerBase
{
    private readonly IProductVariantService _variantService;

    public ProductVariantsController(IProductVariantService variantService)
        => _variantService = variantService;

    [HttpGet]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetByProduct(Guid productId)
    {
        var variants = await _variantService.GetByProductAsync(productId);
        return Ok(variants);
    }

    [HttpGet("{id:guid}")]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var variant = await _variantService.GetByIdAsync(id);
        return variant is null ? NotFound() : Ok(variant);
    }

    [HttpPost]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> Create(Guid productId, CreateVariantRequest request)
    {
        var variant = await _variantService.CreateAsync(productId, request);
        return CreatedResponse(variant);
    }

    [HttpPut("{id:guid}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> Update(Guid id, UpdateVariantRequest request)
    {
        await _variantService.UpdateAsync(id, request);
        return UpdatedResponse();
    }

    [HttpDelete("{id:guid}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _variantService.DeleteAsync(id);
        return DeletedResponse();
    }
}