

using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.Catalog.Interfaces;

namespace SaaSPlatform.Application.Services;

public class ProductManagementService
{
    private readonly IProductRepository _productRepo;

    public ProductManagementService(IProductRepository productRepo) => _productRepo = productRepo;

    public async Task<List<Product>> GetStoreProductsAsync(Guid storeId)
        => await _productRepo.GetByStoreAsync(storeId);

    public async Task<Product?> GetProductAsync(Guid productId)
        => await _productRepo.GetByIdAsync(productId);

    public async Task<Product> CreateProductAsync(Product product)
    {
        _productRepo.Add(product);
        // If you're not inside a unit-of-work, you'll need to save here.
        // The best practice is to inject IUnitOfWork and call SaveChangesAsync().
        return product;
    }

    public async Task UpdateProductAsync(Product product)
    {
        _productRepo.Update(product);
    }

    public async Task DeleteProductAsync(Guid productId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product != null)
            _productRepo.Delete(product);
    }
}