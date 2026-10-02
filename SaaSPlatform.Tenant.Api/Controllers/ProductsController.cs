using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Services;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/products")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class ProductsController : TenantApiControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService, ITenantContext tenantContext)
        : base(tenantContext)
    {
        _productService = productService;
    }

    /// <summary>All products of the tenant's menu, with optional search and filters.</summary>
    [HttpGet]
    [RequiresPermission(Permissions.Catalog.ProductsView)]
    public async Task<IActionResult> Search([FromQuery] ProductFilterQuery filter, CancellationToken ct)
    {
        var result = await _productService.SearchAsync(RequireTenantId(), filter, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(RequireTenantId(), id, ct);
        return product is null ? NotFound() : OkResponse(product);
    }

    [HttpPost]
    [EnableRateLimiting("product-create")]
    [RequiresPermission(Permissions.Catalog.ProductsCreate)]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken ct)
    {
        var product = await _productService.CreateAsync(RequireTenantId(), request, ct);
        return CreatedResponse(product);
    }

    [HttpPut("{id:guid}")]
    [EnableRateLimiting("product-update")]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = await _productService.UpdateAsync(RequireTenantId(), id, request, ct);
        return OkResponse(product);
    }

    [HttpDelete("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _productService.DeleteAsync(RequireTenantId(), id, ct);
        return DeletedResponse();
    }
}