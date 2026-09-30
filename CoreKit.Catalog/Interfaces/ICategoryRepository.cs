using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreKit.Catalog.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<List<Category>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> HasChildrenAsync(Guid tenantId, Guid categoryId, CancellationToken ct = default);
    Task<bool> HasProductsAsync(Guid tenantId, Guid categoryId, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<HashSet<string>> GetSlugsStartingWithAsync(Guid tenantId, string prefix, CancellationToken ct = default);

    void Add(Category category);
    void Remove(Category category);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    Task ExecuteAdvisoryLockAsync(long lockKey, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}