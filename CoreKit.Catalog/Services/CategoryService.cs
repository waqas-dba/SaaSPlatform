using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Helpers;
using CoreKit.Tenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

public class CategoryService : ICategoryService
{
    private readonly CatalogDbContext _db;
    private readonly IPlanLimitProvider? _planLimit;

    public CategoryService(CatalogDbContext db, IPlanLimitProvider? planLimit = null)
    {
        _db = db;
        _planLimit = planLimit;
    }

    public async Task<List<CategoryDto>> GetByTenantAsync(
        Guid tenantId,
        Guid? storeId = null,
        CancellationToken ct = default)
    {
        var query = _db.Categories
            .Where(c => c.TenantId == tenantId &&
                        (storeId == null || c.StoreId == storeId))
            .Include(c => c.SubCategories);

        return await query
            .Select(c => MapToDto(c))
            .ToListAsync(ct);
    }

    // Keep non-ct overload to satisfy existing interface contract
    Task<List<CategoryDto>> ICategoryService.GetByTenantAsync(
        Guid tenantId, Guid? storeId)
        => GetByTenantAsync(tenantId, storeId, CancellationToken.None);

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _db.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        return category is null ? null : MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        if (_planLimit is not null && request.ParentCategoryId.HasValue)
        {
            var maxLevel = await _planLimit.GetMaxCategoryLevelAsync(request.TenantId);
            if (maxLevel > 0)
            {
                var parentLevel = await _db.Categories
                    .Where(c => c.Id == request.ParentCategoryId.Value)
                    .Select(c => c.Level)
                    .FirstOrDefaultAsync();

                if (parentLevel >= maxLevel)
                    throw new ForbiddenException(
                        $"Category depth limited to {maxLevel} level(s) " +
                        "by your subscription.");
            }
        }

        int level = 1;
        if (request.ParentCategoryId.HasValue)
        {
            var parent = await _db.Categories.FindAsync(request.ParentCategoryId.Value)
                ?? throw new KeyNotFoundException("Parent category not found.");
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
        await _db.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        if (request.Name is not null)
        {
            category.Name = request.Name;
            category.Slug = SlugHelper.Generate(request.Name);
        }

        if (request.IconUrl is not null) category.IconUrl = request.IconUrl;
        if (request.StoreTypeCode is not null) category.StoreTypeCode = request.StoreTypeCode;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _db.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Category not found.");

        if (category.SubCategories.Any())
            throw new InvalidOperationException(
                "Cannot delete a category with subcategories.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            IconUrl = category.IconUrl,
            ParentCategoryId = category.ParentCategoryId,
            Level = category.Level,
            StoreTypeCode = category.StoreTypeCode,
            StoreId = category.StoreId,
            TenantId = category.TenantId,
            SubCategories = category.SubCategories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Level = c.Level
            }).ToList()
        };
    }
}