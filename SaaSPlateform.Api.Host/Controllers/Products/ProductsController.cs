using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Api.Host.Controllers;
using SaaSPlatform.Application.Services;
using SaaSPlatform.Core.Catalog.Entities;

[ApiController]
[Route("api/products")]
public class ProductsController : BaseApiController
{
    private readonly ProductManagementService _productService;

    public ProductsController(ProductManagementService productService)
        => _productService = productService;

    [HttpGet]
    public async Task<IActionResult> GetProducts(Guid storeId)
    {
        var products = await _productService.GetStoreProductsAsync(storeId);
        return Success(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        var created = await _productService.CreateProductAsync(product);
        return Success(created);
    }
}