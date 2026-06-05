// CoreKit.Catalog/Persistence/CategoryRepository.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreKit.Catalog.Persistence;

public class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _db;
    public CategoryRepository(CatalogDbContext db) => _db = db;

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _db.Categories.AnyAsync(c => c.Id == id, ct);

    public async Task<List<Category>> GetTreeAsync(
        Guid tenantId, Guid? storeId, CancellationToken ct)
    {
        var all = await _db.Categories
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId &&
                        c.StoreId == storeId)
            .OrderBy(c => c.Level).ThenBy(c => c.Name)
            .ToListAsync(ct);
        return all;
    }

    public async Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken ct)
        => await _db.Categories.AnyAsync(c => c.ParentCategoryId == categoryId, ct);

    // FIX: ignore soft‑deleted products when checking if a category has products
    public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken ct = default)
        => await _db.Products
            .IgnoreQueryFilters()                     // include soft‑deleted
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)   // only active products
            .AnyAsync(ct);

    public void Add(Category category) => _db.Categories.Add(category);
    public void Update(Category category) => _db.Categories.Update(category);
    public void Delete(Category category) => _db.Categories.Remove(category);
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => _db.Database.BeginTransactionAsync(ct);
    public Task CommitAsync(CancellationToken ct = default)
        => _db.Database.CommitTransactionAsync(ct);
    public Task RollbackAsync(CancellationToken ct = default)
        => _db.Database.RollbackTransactionAsync(ct);
    public Task ExecuteAdvisoryLockAsync(long lockKey, CancellationToken ct = default)
        => _db.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock({0})", lockKey);
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}