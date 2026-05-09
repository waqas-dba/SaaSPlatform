using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.Catalog.Interfaces;
using SaaSPlatform.SharedKernel.Interfaces;

namespace SaaSPlatform.Application.Services;

public class ProductManagementService
{
    private readonly IProductRepository _productRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ProductManagementService(IProductRepository productRepo, IUnitOfWork unitOfWork)
    {
        _productRepo = productRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Product>> GetStoreProductsAsync(Guid storeId)
        => await _productRepo.GetByStoreAsync(storeId);

    public async Task<Product?> GetProductAsync(Guid productId)
        => await _productRepo.GetByIdAsync(productId);

    public async Task<Product> CreateProductAsync(Product product)
    {
        _productRepo.Add(product);
        await _unitOfWork.SaveChangesAsync();
        return product;
    }

    public async Task UpdateProductAsync(Product product)
    {
        _productRepo.Update(product);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(Guid productId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product != null)
        {
            _productRepo.Delete(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}