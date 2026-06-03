using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence;

internal sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _db;

    public ProductRepository(CatalogDbContext db) => _db = db;

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => _db.Products
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.AttributeValues).ThenInclude(av => av.Template)
            .Include(p => p.Variants).ThenInclude(v => v.AttributeValues).ThenInclude(va => va.Template)
            .Include(p => p.AddonGroup).ThenInclude(ag => ag!.Addons.Where(a => a.IsActive))
            .Include(p => p.VariantGroup).ThenInclude(vg => vg!.Options).ThenInclude(o => o.Template)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetByStoreAsync(Guid storeId, CancellationToken ct = default)
    {
        var list = await _db.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.AttributeValues).ThenInclude(av => av.Template)
            .Include(p => p.Variants).ThenInclude(v => v.AttributeValues).ThenInclude(va => va.Template)
            .Include(p => p.AddonGroup).ThenInclude(ag => ag!.Addons.Where(a => a.IsActive))
            .Include(p => p.VariantGroup).ThenInclude(vg => vg!.Options).ThenInclude(o => o.Template)
            .Where(p => p.StoreId == storeId)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        return list;
    }

    public Task<int> CountByStoreAsync(Guid storeId, CancellationToken ct = default)
        => _db.Products.CountAsync(p => p.StoreId == storeId, ct);

    public Task<bool> ExistsBySlugAsync(string slug, Guid storeId, CancellationToken ct = default)
        => _db.Products.AnyAsync(p => p.Slug == slug && p.StoreId == storeId, ct);

    public void Add(Product product) => _db.Products.Add(product);
    public void Update(Product product) => _db.Products.Update(product);
    public void Remove(Product product) => _db.Products.Remove(product);

    public Task BeginTransactionAsync(CancellationToken ct = default)
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