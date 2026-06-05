// CoreKit.Catalog | Services/CategoryService.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Helpers;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace CoreKit.Catalog.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IPlanLimitProvider? _planLimit;
    private readonly ILogger<CategoryService> _logger;
    public CategoryService(
        ICategoryRepository categoryRepo,
        ILogger<CategoryService> logger,
        IPlanLimitProvider? planLimit = null)
    {
        _categoryRepo = categoryRepo;
        _logger = logger;
        _planLimit = planLimit;
    }
    public async Task<List<CategoryDto>> GetByTenantAsync(
        Guid tenantId,
        Guid? storeId = null,
        CancellationToken ct = default)
    {
        var all = await _categoryRepo.GetTreeAsync(tenantId, storeId, ct);
        return BuildTree(all, parentId: null);
    }
    public async Task<CategoryDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var root = await _categoryRepo.GetByIdAsync(id, ct);
        if (root is null) return null;
        var allTenant = await _categoryRepo.GetTreeAsync(root.TenantId, root.StoreId, ct);
        var subtree = ExtractSubtree(allTenant, id);
        return MapToDto(subtree.First(c => c.Id == id), subtree);
    }
    public async Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken ct = default)
    {
        var lockKey = LockKeyHelper.GuidToLockKey(request.TenantId);
        using var tx = await _categoryRepo.BeginTransactionAsync(ct);
        try
        {
            await _categoryRepo.ExecuteAdvisoryLockAsync(lockKey, ct);
            int level = 1;
            if (request.ParentCategoryId.HasValue)
            {
                var parent = await _categoryRepo.GetByIdAsync(request.ParentCategoryId.Value, ct)
                    ?? throw new KeyNotFoundException("Parent category not found.");
                if (_planLimit is not null)
                {
                    var maxLevel = await _planLimit.GetMaxCategoryLevelAsync(request.TenantId, ct);
                    if (maxLevel > 0 && parent.Level >= maxLevel)
                        throw new ForbiddenException($"Category depth limited to {maxLevel} level(s) by your subscription.");
                }
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
            _categoryRepo.Add(category);
            await _categoryRepo.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return MapToDto(category, new List<Category>());
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
    public async Task UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken ct = default)
    {
        var category = await _categoryRepo.GetByIdAsync(id, ct)
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
        _categoryRepo.Update(category);
        await _categoryRepo.SaveChangesAsync(ct);
    }
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var hasChildren = await _categoryRepo.HasChildrenAsync(id, ct);
        if (hasChildren)
            throw new InvalidOperationException("Cannot delete a category with subcategories.");

        // FIX: Check for existing products before deletion
        var hasProducts = await _categoryRepo.HasProductsAsync(id, ct);
        if (hasProducts)
            throw new InvalidOperationException("Cannot delete a category that still contains products.");

        var category = await _categoryRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Category not found.");
        _categoryRepo.Delete(category);
        await _categoryRepo.SaveChangesAsync(ct);
    }
    private static List<Category> ExtractSubtree(List<Category> all, Guid rootId)
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
    private static List<CategoryDto> BuildTree(List<Category> all, Guid? parentId)
        => all
            .Where(c => c.ParentCategoryId == parentId)
            .Select(c => MapToDto(c, all))
            .ToList();
    private static CategoryDto MapToDto(Category category, List<Category> all) => new()
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
        SubCategories = BuildTree(all, category.Id)
    };
}