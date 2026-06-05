// CoreKit.Catalog/Persistence/ProductRepository.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreKit.Catalog.Persistence;

public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _db;
    private IDbContextTransaction? _currentTransaction;

    public ProductRepository(CatalogDbContext db) => _db = db;

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => _db.Products
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.AttributeValues).ThenInclude(av => av.Template)
            .Include(p => p.Variants).ThenInclude(v => v.AttributeValues)
                .ThenInclude(va => va.Template)
            .Include(p => p.AddonGroup)
                .ThenInclude(ag => ag!.Addons.Where(a => a.IsActive))
            .Include(p => p.VariantGroup).ThenInclude(vg => vg!.Options)
                .ThenInclude(o => o.Template)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetByStoreAsync(
        Guid storeId, CancellationToken ct = default)
        => await _db.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.AttributeValues).ThenInclude(av => av.Template)
            .Include(p => p.Variants).ThenInclude(v => v.AttributeValues)
                .ThenInclude(va => va.Template)
            .Include(p => p.AddonGroup)
                .ThenInclude(ag => ag!.Addons.Where(a => a.IsActive))
            .Include(p => p.VariantGroup).ThenInclude(vg => vg!.Options)
                .ThenInclude(o => o.Template)
            .Where(p => p.StoreId == storeId)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    public async Task<PagedResult<Product>> GetByStorePagedAsync(
        Guid storeId, PagedQuery query, CancellationToken ct = default)
    {
        var baseQuery = _db.Products
            .AsNoTracking()
            .Where(p => p.StoreId == storeId);

        var totalCount = await baseQuery.CountAsync(ct);
        if (totalCount == 0)
            return PagedResult<Product>.From(
                Array.Empty<Product>(), 0, query.Page, query.PageSize);

        var items = await baseQuery
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.AttributeValues).ThenInclude(av => av.Template)
            .Include(p => p.Variants).ThenInclude(v => v.AttributeValues)
                .ThenInclude(va => va.Template)
            .Include(p => p.AddonGroup)
                .ThenInclude(ag => ag!.Addons.Where(a => a.IsActive))
            .Include(p => p.VariantGroup).ThenInclude(vg => vg!.Options)
                .ThenInclude(o => o.Template)
            .OrderBy(p => p.Name)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return PagedResult<Product>.From(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<PagedResult<ProductListDto>> GetByStorePagedProjectedAsync(
        Guid storeId, PagedQuery query, CancellationToken ct = default)
    {
        var baseQuery = _db.Products.Where(p => p.StoreId == storeId);

        var totalCount = await baseQuery.CountAsync(ct);
        if (totalCount == 0)
            return PagedResult<ProductListDto>.From(
                Array.Empty<ProductListDto>(), 0, query.Page, query.PageSize);

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
                PrimaryImageUrl = p.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),
                IsActive = p.IsActive
            })
            .ToListAsync(ct);

        return PagedResult<ProductListDto>.From(
            items, totalCount, query.Page, query.PageSize);
    }

    public async Task<PagedResult<ProductListDto>> SearchAsync(
        Guid storeId, ProductFilterQuery filter, CancellationToken ct = default)
    {
        var query = _db.Products
            .AsNoTracking()
            .Where(p => p.StoreId == storeId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.Trim();
            // websearch_to_tsquery handles arbitrary user input safely
            // (handles special chars, unbalanced quotes, etc.)
            query = query.Where(p =>
                p.SearchVector.Matches(
                    EF.Functions.WebSearchToTsQuery("english", searchTerm)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.BasePrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.BasePrice <= filter.MaxPrice.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(p => p.IsActive == filter.IsActive.Value);

        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0)
            return PagedResult<ProductListDto>.From(
                Array.Empty<ProductListDto>(), 0, filter.Page, filter.PageSize);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                BasePrice = p.BasePrice,
                CategoryName = p.Category.Name,
                PrimaryImageUrl = p.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),
                IsActive = p.IsActive
            })
            .ToListAsync(ct);

        return PagedResult<ProductListDto>.From(
            items, totalCount, filter.Page, filter.PageSize);
    }

    public Task<int> CountByStoreAsync(Guid storeId, CancellationToken ct = default)
        => _db.Products.CountAsync(p => p.StoreId == storeId, ct);

    public Task<bool> ExistsBySlugAsync(
        string slug, Guid storeId, CancellationToken ct = default)
        => _db.Products.AnyAsync(
            p => p.Slug == slug && p.StoreId == storeId, ct);

    public void Add(Product product) => _db.Products.Add(product);
    public void Update(Product product) => _db.Products.Update(product);
    public void Remove(Product product) => _db.Products.Remove(product);

    public async Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken ct = default)
    {
        _currentTransaction = await _db.Database.BeginTransactionAsync(ct);
        return _currentTransaction;
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException(
                "No active transaction to commit.");
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
        => _db.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock({0})", lockKey);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}