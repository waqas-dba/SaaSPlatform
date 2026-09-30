using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Helpers;
using CoreKit.SharedKernel.Interfaces;

namespace CoreKit.Catalog.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IPlanLimitProvider? _planLimit;

    public CategoryService(ICategoryRepository categoryRepo, IPlanLimitProvider? planLimit = null)
    {
        _categoryRepo = categoryRepo;
        _planLimit = planLimit;
    }

    public async Task<List<CategoryDto>> GetTreeAsync(Guid tenantId, CancellationToken ct = default)
    {
        var all = await _categoryRepo.GetAllAsync(tenantId, ct);
        var byParent = all.ToLookup(c => c.ParentCategoryId);

        return byParent[null].Select(c => BuildNode(c, byParent)).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var all = await _categoryRepo.GetAllAsync(tenantId, ct);
        var root = all.FirstOrDefault(c => c.Id == id);
        if (root is null) return null;

        var byParent = all.ToLookup(c => c.ParentCategoryId);
        return BuildNode(root, byParent);
    }

    public async Task<CategoryDto> CreateAsync(
        Guid tenantId, CreateCategoryRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        if (name.Length == 0)
            throw new ArgumentException("Category name is required.");

        await _categoryRepo.BeginTransactionAsync(ct);
        try
        {
            await _categoryRepo.ExecuteAdvisoryLockAsync(LockKeyHelper.GuidToLockKey(tenantId), ct);
            await EnforceCategoryCountLimitAsync(tenantId, ct);

            var level = 1;
            if (request.ParentCategoryId.HasValue)
            {
                var parent = await _categoryRepo.GetByIdAsync(tenantId, request.ParentCategoryId.Value, ct)
                    ?? throw new KeyNotFoundException("Parent category not found.");

                if (_planLimit is not null)
                {
                    var maxLevel = await _planLimit.GetMaxCategoryLevelAsync(tenantId, ct);
                    if (maxLevel > 0 && parent.Level >= maxLevel)
                        throw new ForbiddenException(
                            $"Category depth is limited to {maxLevel} level(s) by your subscription.");
                }

                level = parent.Level + 1;
            }

            var baseSlug = CatalogGuards.BaseSlug(name);
            var taken = await _categoryRepo.GetSlugsStartingWithAsync(tenantId, baseSlug, ct);

            var category = new Category
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name,
                Slug = CatalogGuards.PickUniqueSlug(baseSlug, taken),
                IconUrl = request.IconUrl,
                SortOrder = request.SortOrder,
                ParentCategoryId = request.ParentCategoryId,
                Level = level
            };

            _categoryRepo.Add(category);
            await _categoryRepo.SaveChangesAsync(ct);
            await _categoryRepo.CommitAsync(ct);

            return CatalogMapper.ToDto(category);
        }
        catch
        {
            await _categoryRepo.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<CategoryDto> UpdateAsync(
        Guid tenantId, Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _categoryRepo.GetByIdAsync(tenantId, id, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        if (request.Name is not null)
        {
            var name = request.Name.Trim();
            if (name.Length == 0)
                throw new ArgumentException("Category name cannot be empty.");

            if (!string.Equals(name, category.Name, StringComparison.Ordinal))
            {
                var baseSlug = CatalogGuards.BaseSlug(name);
                var taken = await _categoryRepo.GetSlugsStartingWithAsync(tenantId, baseSlug, ct);
                taken.Remove(category.Slug);

                category.Name = name;
                category.Slug = CatalogGuards.PickUniqueSlug(baseSlug, taken);
            }
        }

        if (request.IconUrl is not null) category.IconUrl = request.IconUrl;
        if (request.SortOrder.HasValue) category.SortOrder = request.SortOrder.Value;
        if (request.IsActive.HasValue) category.IsActive = request.IsActive.Value;

        await _categoryRepo.SaveChangesAsync(ct);
        return CatalogMapper.ToDto(category);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var category = await _categoryRepo.GetByIdAsync(tenantId, id, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        if (await _categoryRepo.HasChildrenAsync(tenantId, id, ct))
            throw new InvalidOperationException("Cannot delete a category with subcategories.");

        if (await _categoryRepo.HasProductsAsync(tenantId, id, ct))
            throw new InvalidOperationException("Cannot delete a category that still contains products.");

        _categoryRepo.Remove(category);
        await _categoryRepo.SaveChangesAsync(ct);
    }

    private async Task EnforceCategoryCountLimitAsync(Guid tenantId, CancellationToken ct)
    {
        if (_planLimit is null) return;

        var max = await _planLimit.GetMaxCategoriesAsync(tenantId, ct);
        if (!max.HasValue) return;

        var count = await _categoryRepo.CountByTenantAsync(tenantId, ct);
        if (count >= max.Value)
            throw new InvalidOperationException("Category limit reached. Upgrade your plan.");
    }

    private static CategoryDto BuildNode(Category category, ILookup<Guid?, Category> byParent)
    {
        var dto = CatalogMapper.ToDto(category);
        dto.SubCategories = byParent[category.Id].Select(c => BuildNode(c, byParent)).ToList();
        return dto;
    }
}