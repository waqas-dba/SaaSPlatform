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

[ApiController]
[Route("api/v{version:apiVersion}/products/{productId:guid}/variants")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class ProductVariantsController : TenantApiControllerBase
{
    private readonly IProductVariantService _variantService;

    public ProductVariantsController(IProductVariantService variantService, ITenantContext tenantContext)
        : base(tenantContext)
    {
        _variantService = variantService;
    }

    [HttpGet]
    [RequiresPermission(Permissions.Catalog.ProductsView)]
    public async Task<IActionResult> GetByProduct(Guid productId, CancellationToken ct)
    {
        var variants = await _variantService.GetByProductAsync(RequireTenantId(), productId, ct);
        return OkResponse(variants);
    }

    [HttpGet("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsView)]
    public async Task<IActionResult> GetById(Guid productId, Guid id, CancellationToken ct)
    {
        var variant = await _variantService.GetByIdAsync(RequireTenantId(), productId, id, ct);
        return variant is null ? NotFound() : OkResponse(variant);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> Create(Guid productId, CreateVariantRequest request, CancellationToken ct)
    {
        var variant = await _variantService.CreateAsync(RequireTenantId(), productId, request, ct);
        return CreatedResponse(variant);
    }

    [HttpPut("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> Update(
        Guid productId, Guid id, UpdateVariantRequest request, CancellationToken ct)
    {
        var variant = await _variantService.UpdateAsync(RequireTenantId(), productId, id, request, ct);
        return OkResponse(variant);
    }

    [HttpDelete("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> Delete(Guid productId, Guid id, CancellationToken ct)
    {
        await _variantService.DeleteAsync(RequireTenantId(), productId, id, ct);
        return DeletedResponse();
    }
}