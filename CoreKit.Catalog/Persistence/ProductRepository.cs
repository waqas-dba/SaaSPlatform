using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence;

public class ProductRepository : IProductRepository
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

    public async Task<PagedResult<Product>> GetByStorePagedAsync(Guid storeId, PagedQuery query, CancellationToken ct = default)
    {
        var baseQuery = _db.Products.AsNoTracking().Where(p => p.StoreId == storeId);
        var totalCount = await baseQuery.CountAsync(ct);
        if (totalCount == 0)
            return PagedResult<Product>.From(Array.Empty<Product>(), 0, query.Page, query.PageSize);

        var items = await baseQuery
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.AttributeValues).ThenInclude(av => av.Template)
            .Include(p => p.Variants).ThenInclude(v => v.AttributeValues).ThenInclude(va => va.Template)
            .Include(p => p.AddonGroup).ThenInclude(ag => ag!.Addons.Where(a => a.IsActive))
            .Include(p => p.VariantGroup).ThenInclude(vg => vg!.Options).ThenInclude(o => o.Template)
            .OrderBy(p => p.Name)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return PagedResult<Product>.From(items, totalCount, query.Page, query.PageSize);
    }

    // Lightweight projected query (NEW – only one definition here)
    public async Task<PagedResult<ProductListDto>> GetByStorePagedProjectedAsync(Guid storeId, PagedQuery query, CancellationToken ct = default)
    {
        var baseQuery = _db.Products.Where(p => p.StoreId == storeId);
        var totalCount = await baseQuery.CountAsync(ct);
        if (totalCount == 0)
            return PagedResult<ProductListDto>.From(Array.Empty<ProductListDto>(), 0, query.Page, query.PageSize);

        var items = await baseQuery
            .OrderBy(p => p.Name)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                BasePrice = p.BasePrice,
                CategoryName = p.Category.Name,
                PrimaryImageUrl = p.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                IsActive = p.IsActive
            })
            .ToListAsync(ct);

        return PagedResult<ProductListDto>.From(items, totalCount, query.Page, query.PageSize);
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