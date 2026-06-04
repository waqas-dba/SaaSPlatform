using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Persistence;
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

    public async Task<List<Category>> GetTreeAsync(Guid tenantId, Guid? storeId, CancellationToken ct)
    {
        // Build SQL dynamically to avoid parameter issues with null
        string sql;
        object[] parameters;

        if (storeId.HasValue)
        {
            sql = @"
                WITH RECURSIVE category_tree AS (
                    SELECT * FROM ""Catalog_Categories""
                    WHERE ""TenantId"" = {0} AND ""StoreId"" = {1} AND ""ParentCategoryId"" IS NULL
                    UNION ALL
                    SELECT c.* FROM ""Catalog_Categories"" c
                    INNER JOIN category_tree ct ON c.""ParentCategoryId"" = ct.""Id""
                )
                SELECT * FROM category_tree ORDER BY ""Level"", ""Name""";
            parameters = new object[] { tenantId, storeId.Value };
        }
        else
        {
            sql = @"
                WITH RECURSIVE category_tree AS (
                    SELECT * FROM ""Catalog_Categories""
                    WHERE ""TenantId"" = {0} AND ""StoreId"" IS NULL AND ""ParentCategoryId"" IS NULL
                    UNION ALL
                    SELECT c.* FROM ""Catalog_Categories"" c
                    INNER JOIN category_tree ct ON c.""ParentCategoryId"" = ct.""Id""
                )
                SELECT * FROM category_tree ORDER BY ""Level"", ""Name""";
            parameters = new object[] { tenantId };
        }

        return await _db.Categories
            .FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken ct)
        => await _db.Categories.AnyAsync(c => c.ParentCategoryId == categoryId, ct);

    public void Add(Category category) => _db.Categories.Add(category);
    public void Update(Category category) => _db.Categories.Update(category);
    public void Delete(Category category) => _db.Categories.Remove(category);

    // FIXED: return type matches interface
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => _db.Database.BeginTransactionAsync(ct);

    public Task CommitAsync(CancellationToken ct = default)
        => _db.Database.CommitTransactionAsync(ct);

    public Task RollbackAsync(CancellationToken ct = default)
        => _db.Database.RollbackTransactionAsync(ct);

    public Task ExecuteAdvisoryLockAsync(long lockKey, CancellationToken ct = default)
        => _db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0})", lockKey);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}