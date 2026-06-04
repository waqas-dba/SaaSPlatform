using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/products/{productId}/variants")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class ProductVariantsController : ApiControllerBase
{
    private readonly IProductVariantService _variantService;

    public ProductVariantsController(IProductVariantService variantService)
        => _variantService = variantService;

    [HttpGet]
    [RequiresPermission(Permissions.Catalog.ProductsView)]
    public async Task<IActionResult> GetByProduct(Guid productId, CancellationToken ct)
    {
        var variants = await _variantService.GetByProductAsync(productId);
        return OkResponse(variants);
    }

    [HttpGet("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var variant = await _variantService.GetByIdAsync(id);
        return variant is null ? NotFound() : OkResponse(variant);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> Create(Guid productId, CreateVariantRequest request, CancellationToken ct)
    {
        var variant = await _variantService.CreateAsync(productId, request);
        return CreatedResponse(variant);
    }

    [HttpPut("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> Update(Guid id, UpdateVariantRequest request, CancellationToken ct)
    {
        await _variantService.UpdateAsync(id, request);
        return UpdatedResponse();
    }

    [HttpDelete("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _variantService.DeleteAsync(id);
        return DeletedResponse();
    }
}