using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Helpers;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

public class CategoryService : ICategoryService
{
    private readonly CatalogDbContext _db;
    private readonly IPlanLimitProvider? _planLimit;

    public CategoryService(
        CatalogDbContext db,
        IPlanLimitProvider? planLimit = null)
    {
        _db = db;
        _planLimit = planLimit;
    }

    public async Task<List<CategoryDto>> GetByTenantAsync(
        Guid tenantId,
        Guid? storeId = null,
        CancellationToken ct = default)
    {
        var all = await _db.Categories
            .Where(c =>
                c.TenantId == tenantId &&
                (storeId == null || c.StoreId == storeId))
            .ToListAsync(ct);

        // Build tree starting from root (no parent) — Level on each
        // entity is the authoritative value written at creation time.
        return BuildTree(all, parentId: null);
    }

    Task<List<CategoryDto>> ICategoryService.GetByTenantAsync(
        Guid tenantId, Guid? storeId)
        => GetByTenantAsync(tenantId, storeId, CancellationToken.None);

    public async Task<CategoryDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        // Load the entire tenant subtree that contains this category
        // so BuildTree can walk to any depth. We first find the root
        // category's tenantId, then load all categories for that tenant
        // that are descendants of (or equal to) the requested id.
        var root = await _db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (root is null) return null;

        // Load all categories in the same tenant that belong to the
        // subtree rooted at `id` by fetching the full tenant set and
        // filtering in memory — avoids a recursive CTE while keeping
        // a single round-trip.
        var allTenant = await _db.Categories
            .AsNoTracking()
            .Where(c => c.TenantId == root.TenantId)
            .ToListAsync(ct);

        var subtree = ExtractSubtree(allTenant, id);

        return MapToDto(subtree.First(c => c.Id == id), subtree);
    }

    Task<CategoryDto?> ICategoryService.GetByIdAsync(Guid id)
        => GetByIdAsync(id, CancellationToken.None);

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken ct = default)
    {
        var lockKey = LockKeyHelper.GuidToLockKey(request.TenantId);
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock({0})", lockKey);

            int level = 1;

            if (request.ParentCategoryId.HasValue)
            {
                var parent = await _db.Categories
                    .FirstOrDefaultAsync(
                        c => c.Id == request.ParentCategoryId.Value, ct)
                    ?? throw new KeyNotFoundException(
                        "Parent category not found.");

                if (_planLimit is not null)
                {
                    var maxLevel = await _planLimit
                        .GetMaxCategoryLevelAsync(request.TenantId, ct);

                    // 0 means unlimited
                    if (maxLevel > 0 && parent.Level >= maxLevel)
                        throw new ForbiddenException(
                            $"Category depth limited to {maxLevel} level(s) " +
                            "by your subscription.");
                }

                // Level is derived dynamically from the parent so that
                // moving categories in the future only requires updating
                // the parent pointer, not rewriting all descendants.
                level = parent.Level + 1;
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = SlugHelper.Generate(request.Name),
                IconUrl = request.IconUrl,
                ParentCategoryId = request.ParentCategoryId,
                StoreTypeCode = request.StoreTypeCode,
                StoreId = request.StoreId,
                TenantId = request.TenantId,
                Level = level
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return MapToDto(category, new List<Category>());
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    Task<CategoryDto> ICategoryService.CreateAsync(CreateCategoryRequest request)
        => CreateAsync(request, CancellationToken.None);

    public async Task UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken ct = default)
    {
        var category = await _db.Categories
            .FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        if (request.Name is not null)
        {
            category.Name = request.Name;
            category.Slug = SlugHelper.Generate(request.Name);
        }

        if (request.IconUrl is not null)
            category.IconUrl = request.IconUrl;

        if (request.StoreTypeCode is not null)
            category.StoreTypeCode = request.StoreTypeCode;

        await _db.SaveChangesAsync(ct);
    }

    Task ICategoryService.UpdateAsync(Guid id, UpdateCategoryRequest request)
        => UpdateAsync(id, request, CancellationToken.None);

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var hasChildren = await _db.Categories
            .AnyAsync(c => c.ParentCategoryId == id, ct);

        if (hasChildren)
            throw new InvalidOperationException(
                "Cannot delete a category with subcategories.");

        var category = await _db.Categories
            .FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);
    }

    Task ICategoryService.DeleteAsync(Guid id)
        => DeleteAsync(id, CancellationToken.None);

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    /// <summary>
    /// Returns the node at <paramref name="rootId"/> plus all of its
    /// descendants from <paramref name="all"/>.
    /// </summary>
    private static List<Category> ExtractSubtree(
        List<Category> all, Guid rootId)
    {
        var result = new List<Category>();
        var queue = new Queue<Guid>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var node = all.FirstOrDefault(c => c.Id == current);
            if (node is null) continue;

            result.Add(node);

            foreach (var child in all.Where(c => c.ParentCategoryId == current))
                queue.Enqueue(child.Id);
        }

        return result;
    }

    /// <summary>
    /// Recursively builds a DTO tree from a flat list.
    /// <paramref name="parentId"/> == null selects root nodes.
    /// </summary>
    private static List<CategoryDto> BuildTree(
        List<Category> all,
        Guid? parentId)
        => all
            .Where(c => c.ParentCategoryId == parentId)
            .Select(c => MapToDto(c, all))
            .ToList();

    private static CategoryDto MapToDto(
        Category category,
        List<Category> all) => new()
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            IconUrl = category.IconUrl,
            ParentCategoryId = category.ParentCategoryId,
            Level = category.Level,   // stored at write time, always correct
            StoreTypeCode = category.StoreTypeCode,
            StoreId = category.StoreId,
            TenantId = category.TenantId,
            SubCategories = BuildTree(all, category.Id)
        };
}