using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Core.Catalog.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid productId, CancellationToken ct = default);
    Task<List<Product>> GetByStoreAsync(Guid storeId, CancellationToken ct = default);
    void Add(Product product);
    void Update(Product product);
    void Delete(Product product);
}