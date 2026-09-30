using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreKit.Catalog.Interfaces;

public interface IStoreProductRepository
{
    Task<StoreProduct?> GetAsync(Guid tenantId, Guid storeId, Guid productId, CancellationToken ct = default);
    Task<List<StoreProduct>> GetByStoreAsync(Guid tenantId, Guid storeId, CancellationToken ct = default);
    Task<HashSet<Guid>> GetProductIdsAsync(Guid tenantId, Guid storeId, CancellationToken ct = default);

    void Add(StoreProduct storeProduct);
    void Remove(StoreProduct storeProduct);
    void RemoveRange(IEnumerable<StoreProduct> storeProducts);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    Task ExecuteAdvisoryLockAsync(long lockKey, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}