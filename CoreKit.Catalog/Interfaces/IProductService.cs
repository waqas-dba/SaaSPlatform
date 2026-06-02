using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IProductService
{
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<List<ProductDto>> GetByStoreAsync(Guid storeId);
}