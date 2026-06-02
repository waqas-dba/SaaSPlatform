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
    [RequiresPermission("catalog.products.create")]
    [EnableRateLimiting("product-create")]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);
        return CreatedResponse(product);
    }

    [HttpGet("{id}")]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("store/{storeId}")]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var products = await _productService.GetByStoreAsync(storeId);
        return Ok(products);
    }
}