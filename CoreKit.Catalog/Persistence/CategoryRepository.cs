using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreKit.Catalog.Persistence;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _db;
    private IDbContextTransaction? _currentTransaction;

    public CategoryRepository(CatalogDbContext db) => _db = db;

    public Task<Category?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        => _db.Categories.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id, ct);

    public Task<List<Category>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
        => _db.Categories
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Level)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

    public Task<bool> HasChildrenAsync(Guid tenantId, Guid categoryId, CancellationToken ct = default)
        => _db.Categories.AnyAsync(
            c => c.TenantId == tenantId && c.ParentCategoryId == categoryId, ct);

    public Task<bool> HasProductsAsync(Guid tenantId, Guid categoryId, CancellationToken ct = default)
        => _db.Products.AnyAsync(
            p => p.TenantId == tenantId && p.CategoryId == categoryId, ct);

    public Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _db.Categories.CountAsync(c => c.TenantId == tenantId, ct);

    public async Task<HashSet<string>> GetSlugsStartingWithAsync(
        Guid tenantId, string prefix, CancellationToken ct = default)
    {
        var slugs = await _db.Categories
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && !c.IsDeleted && c.Slug.StartsWith(prefix))
            .Select(c => c.Slug)
            .ToListAsync(ct);

        return new HashSet<string>(slugs, StringComparer.Ordinal);
    }

    public void Add(Category category) => _db.Categories.Add(category);
    public void Remove(Category category) => _db.Categories.Remove(category);

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        _currentTransaction = await _db.Database.BeginTransactionAsync(ct);
        return _currentTransaction;
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        await _currentTransaction.CommitAsync(ct);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null) return;

        await _currentTransaction.RollbackAsync(ct);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public Task ExecuteAdvisoryLockAsync(long lockKey, CancellationToken ct = default)
        => _db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0})", new object[] { lockKey }, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}