using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Application.Services;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Api.Host.Controllers.Products;

[ApiController]
[Route("api/products")]
public class ProductsController : BaseApiController
{
    private readonly ProductManagementService _service;

    public ProductsController(ProductManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid storeId)
    {
        var products = await _service.GetStoreProductsAsync(storeId);
        return Success(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        var created = await _service.CreateProductAsync(product);
        return Success(created);
    }
}