using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore.Storage;   // for IDbContextTransaction

namespace CoreKit.Catalog.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<List<Category>> GetTreeAsync(Guid tenantId, Guid? storeId, CancellationToken ct = default);
    Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken ct = default);

    void Add(Category category);
    void Update(Category category);
    void Delete(Category category);

    // Returns the transaction object
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    Task ExecuteAdvisoryLockAsync(long lockKey, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}