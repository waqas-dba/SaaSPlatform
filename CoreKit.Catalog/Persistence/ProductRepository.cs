using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreKit.Catalog.Persistence;

public sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _db;
    private IDbContextTransaction? _currentTransaction;

    public ProductRepository(CatalogDbContext db) => _db = db;

    public Task<Product?> GetByIdWithDetailsAsync(
        Guid tenantId, Guid id, bool track, CancellationToken ct = default)
    {
        IQueryable<Product> query = _db.Products
            .Where(p => p.TenantId == tenantId && p.Id == id)
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .Include(p => p.AddonGroupLinks)
                .ThenInclude(l => l.AddonGroup)
                    .ThenInclude(g => g.Addons)
            .Include(p => p.StoreProducts);

        if (!track) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(ct);
    }

    public Task<Product?> GetByIdWithVariantsAsync(
        Guid tenantId, Guid id, bool track, CancellationToken ct = default)
    {
        IQueryable<Product> query = _db.Products
            .Where(p => p.TenantId == tenantId && p.Id == id)
            .Include(p => p.Variants);

        if (!track) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(ct);
    }

    public Task<Product?> GetByIdWithImagesAsync(
        Guid tenantId, Guid id, bool track, CancellationToken ct = default)
    {
        IQueryable<Product> query = _db.Products
            .Where(p => p.TenantId == tenantId && p.Id == id)
            .Include(p => p.Images);

        if (!track) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<ProductListDto>> SearchAsync(
        Guid tenantId, ProductFilterQuery filter, CancellationToken ct = default)
    {
        var query = _db.Products
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(p =>
                p.SearchVector.Matches(EF.Functions.WebSearchToTsQuery("english", term)));
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
                new List<ProductListDto>(), 0, filter.Page, filter.PageSize);

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
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                PrimaryImageUrl = p.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),
                IsActive = p.IsActive,
                IsVegetarian = p.IsVegetarian
            })
            .ToListAsync(ct);

        return PagedResult<ProductListDto>.From(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<PagedResult<StoreMenuItemDto>> GetStoreMenuAsync(
        Guid tenantId, Guid storeId, ProductFilterQuery filter, CancellationToken ct = default)
    {
        var query = _db.StoreProducts
            .AsNoTracking()
            .Where(sp => sp.TenantId == tenantId && sp.StoreId == storeId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(sp =>
                sp.Product.SearchVector.Matches(EF.Functions.WebSearchToTsQuery("english", term)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(sp => sp.Product.CategoryId == filter.CategoryId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(sp => (sp.PriceOverride ?? sp.Product.BasePrice) >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(sp => (sp.PriceOverride ?? sp.Product.BasePrice) <= filter.MaxPrice.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(sp => sp.Product.IsActive == filter.IsActive.Value);

        if (filter.IsAvailable.HasValue)
            query = query.Where(sp => sp.IsAvailable == filter.IsAvailable.Value);

        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0)
            return PagedResult<StoreMenuItemDto>.From(
                new List<StoreMenuItemDto>(), 0, filter.Page, filter.PageSize);

        var items = await query
            .OrderBy(sp => sp.Product.Category.SortOrder)
            .ThenBy(sp => sp.SortOrder)
            .ThenBy(sp => sp.Product.Name)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .Select(sp => new StoreMenuItemDto
            {
                ProductId = sp.ProductId,
                Name = sp.Product.Name,
                Slug = sp.Product.Slug,
                CategoryId = sp.Product.CategoryId,
                CategoryName = sp.Product.Category.Name,
                PrimaryImageUrl = sp.Product.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),
                IsVegetarian = sp.Product.IsVegetarian,
                BasePrice = sp.Product.BasePrice,
                PriceOverride = sp.PriceOverride,
                EffectivePrice = sp.PriceOverride ?? sp.Product.BasePrice,
                IsActive = sp.Product.IsActive,
                IsAvailable = sp.IsAvailable,
                SortOrder = sp.SortOrder
            })
            .ToListAsync(ct);

        return PagedResult<StoreMenuItemDto>.From(items, totalCount, filter.Page, filter.PageSize);
    }

    public Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _db.Products.CountAsync(p => p.TenantId == tenantId, ct);

    public async Task<HashSet<string>> GetSlugsStartingWithAsync(
        Guid tenantId, string prefix, CancellationToken ct = default)
    {
        var slugs = await _db.Products
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.Slug.StartsWith(prefix))
            .Select(p => p.Slug)
            .ToListAsync(ct);

        return new HashSet<string>(slugs, StringComparer.Ordinal);
    }

    public async Task<HashSet<Guid>> GetExistingIdsAsync(
        Guid tenantId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
    {
        var found = await _db.Products
            .Where(p => p.TenantId == tenantId && ids.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(ct);

        return new HashSet<Guid>(found);
    }

    public void Add(Product product) => _db.Products.Add(product);
    public void Remove(Product product) => _db.Products.Remove(product);
    public void AddVariant(ProductVariant variant) => _db.ProductVariants.Add(variant);
    public void RemoveVariant(ProductVariant variant) => _db.ProductVariants.Remove(variant);
    public void AddImage(ProductImage image) => _db.ProductImages.Add(image);
    public void RemoveImage(ProductImage image) => _db.ProductImages.Remove(image);
    public void AddAddonLink(ProductAddonGroup link) => _db.ProductAddonGroups.Add(link);
    public void RemoveAddonLink(ProductAddonGroup link) => _db.ProductAddonGroups.Remove(link);

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