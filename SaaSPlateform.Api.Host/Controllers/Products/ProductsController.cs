using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Application.Services;
using SaaSPlatform.Core.Catalog.Entities;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductManagementService _productService;

    public ProductsController(ProductManagementService productService) => _productService = productService;

    [HttpGet]
    public async Task<IActionResult> GetProducts(Guid storeId)
    {
        var products = await _productService.GetStoreProductsAsync(storeId);
        return Ok(ApiResponse<List<Product>>.SuccessResponse(products));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        var created = await _productService.CreateProductAsync(product);
        return Ok(ApiResponse<Product>.SuccessResponse(created));
    }
    // ... PUT, DELETE similarly
}