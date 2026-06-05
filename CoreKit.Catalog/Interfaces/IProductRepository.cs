// CoreKit.Catalog/Interfaces/IProductRepository.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreKit.Catalog.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByStoreAsync(Guid storeId, CancellationToken ct = default);
    Task<PagedResult<Product>> GetByStorePagedAsync(Guid storeId, PagedQuery query, CancellationToken ct = default);
    Task<PagedResult<ProductListDto>> GetByStorePagedProjectedAsync(Guid storeId, PagedQuery query, CancellationToken ct = default);
    Task<PagedResult<ProductListDto>> SearchAsync(Guid storeId, ProductFilterQuery filter, CancellationToken ct = default);
    Task<int> CountByStoreAsync(Guid storeId, CancellationToken ct = default);
    Task<bool> ExistsBySlugAsync(string slug, Guid storeId, CancellationToken ct = default);
    void Add(Product product);
    void Update(Product product);
    void Remove(Product product);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    Task ExecuteAdvisoryLockAsync(long lockKey, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}