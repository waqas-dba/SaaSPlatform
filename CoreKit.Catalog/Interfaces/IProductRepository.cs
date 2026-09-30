using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreKit.Catalog.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdWithDetailsAsync(Guid tenantId, Guid id, bool track, CancellationToken ct = default);
    Task<Product?> GetByIdWithVariantsAsync(Guid tenantId, Guid id, bool track, CancellationToken ct = default);
    Task<Product?> GetByIdWithImagesAsync(Guid tenantId, Guid id, bool track, CancellationToken ct = default);

    Task<PagedResult<ProductListDto>> SearchAsync(Guid tenantId, ProductFilterQuery filter, CancellationToken ct = default);
    Task<PagedResult<StoreMenuItemDto>> GetStoreMenuAsync(Guid tenantId, Guid storeId, ProductFilterQuery filter, CancellationToken ct = default);

    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<HashSet<string>> GetSlugsStartingWithAsync(Guid tenantId, string prefix, CancellationToken ct = default);
    Task<HashSet<Guid>> GetExistingIdsAsync(Guid tenantId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    void Add(Product product);
    void Remove(Product product);
    void AddVariant(ProductVariant variant);
    void RemoveVariant(ProductVariant variant);
    void AddImage(ProductImage image);
    void RemoveImage(ProductImage image);
    void AddAddonLink(ProductAddonGroup link);
    void RemoveAddonLink(ProductAddonGroup link);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    Task ExecuteAdvisoryLockAsync(long lockKey, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}