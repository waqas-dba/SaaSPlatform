using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;                     // added
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/products")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class ProductsController : ApiControllerBase
{
    private readonly IProductService _productService;
    public ProductsController(IProductService productService) => _productService = productService;

    [HttpPost]
    [EnableRateLimiting("product-create")]
    [RequiresPermission(Permissions.Catalog.ProductsCreate)]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken ct)
    {
        var product = await _productService.CreateAsync(request, ct);
        return CreatedResponse(product);
    }

    [HttpPut("{id:guid}")]
    [EnableRateLimiting("product-update")]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = await _productService.UpdateAsync(id, request, ct);
        return OkResponse(product);
    }

    [HttpGet("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        return product is null ? NotFound() : OkResponse(product);
    }

    [HttpGet("store/{storeId:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsView)]
    public async Task<IActionResult> GetByStore(Guid storeId, [FromQuery] PagedQuery query, CancellationToken ct)
    {
        var result = await _productService.GetByStorePagedAsync(storeId, query, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _productService.DeleteAsync(id, ct);
        return DeletedResponse();
    }
}