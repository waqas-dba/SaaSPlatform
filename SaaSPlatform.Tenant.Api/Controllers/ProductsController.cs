// SaaSPlatform.Tenant.Api/Controllers/ProductsController.cs
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ApiControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
        => _productService = productService;

    [HttpPost]
    [EnableRateLimiting("product-create")]
    [RequiresPermission("catalog.products.create")]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);
        return CreatedResponse(product);
    }

    [HttpPut("{id:guid}")]
    [EnableRateLimiting("product-update")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateProductRequest request)
    {
        var product = await _productService.UpdateAsync(id, request);
        return OkResponse(product);
    }

    [HttpGet("{id:guid}")]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        return product is null ? NotFound() : OkResponse(product);
    }

    [HttpGet("store/{storeId:guid}")]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var products = await _productService.GetByStoreAsync(storeId);
        return OkResponse(products);
    }

    [HttpDelete("{id:guid}")]
    [RequiresPermission("catalog.products.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _productService.DeleteAsync(id);
        return DeletedResponse();
    }
}