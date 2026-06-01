// CoreKit.Catalog/Services/ProductAttributeGroupService.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

public class ProductAttributeGroupService : IProductAttributeGroupService
{
    private readonly CatalogDbContext _db;

    public ProductAttributeGroupService(CatalogDbContext db) => _db = db;

    public async Task<ProductAttributeGroupDto> CreateAsync(
        CreateAttributeGroupRequest request)
    {
        var exists = await _db.AttributeGroups.AnyAsync(g =>
            g.StoreTypeCode == request.StoreTypeCode &&
            g.TenantId == request.TenantId &&
            g.Name == request.Name);

        if (exists)
            throw new InvalidOperationException(
                "An attribute group with this name already exists for the store type.");

        var group = new ProductAttributeGroup
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            StoreTypeCode = request.StoreTypeCode,
            SortOrder = request.SortOrder,
            TenantId = request.TenantId
        };

        _db.AttributeGroups.Add(group);
        await _db.SaveChangesAsync();

        return MapToDto(group);
    }

    public async Task UpdateAsync(Guid groupId, string newName, int sortOrder)
    {
        var group = await _db.AttributeGroups.FindAsync(groupId)
            ?? throw new KeyNotFoundException("Attribute group not found.");

        group.Name = newName;
        group.SortOrder = sortOrder;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid groupId)
    {
        var group = await _db.AttributeGroups
            .Include(g => g.Attributes)
            .FirstOrDefaultAsync(g => g.Id == groupId)
            ?? throw new KeyNotFoundException("Attribute group not found.");

        // Detach attributes from the group rather than deleting them.
        foreach (var attr in group.Attributes)
            attr.GroupId = null;

        _db.AttributeGroups.Remove(group);
        await _db.SaveChangesAsync();
    }

    public async Task<List<ProductAttributeGroupDto>> GetByStoreTypeAsync(
        string storeTypeCode,
        Guid? tenantId = null)
    {
        return await _db.AttributeGroups
            .Where(g =>
                g.StoreTypeCode == storeTypeCode &&
                g.TenantId == tenantId)
            .Include(g => g.Attributes)
            .OrderBy(g => g.SortOrder)
            .Select(g => MapToDto(g))
            .ToListAsync();
    }

    private static ProductAttributeGroupDto MapToDto(ProductAttributeGroup g) =>
        new()
        {
            Id = g.Id,
            Name = g.Name,
            StoreTypeCode = g.StoreTypeCode,
            SortOrder = g.SortOrder,
            TenantId = g.TenantId,
            Attributes = g.Attributes
                .OrderBy(a => a.SortOrder)
                .Select(MapAttrToDto)
                .ToList()
        };

    private static ProductAttributeTemplateDto MapAttrToDto(
        ProductAttributeTemplate a) =>
        new()
        {
            Id = a.Id,
            Name = a.Name,
            StoreTypeCode = a.StoreTypeCode,
            FieldType = a.FieldType,
            OptionsJson = a.OptionsJson,
            IsRequired = a.IsRequired,
            IsVisible = a.IsVisible,
            SortOrder = a.SortOrder,
            TenantId = a.TenantId,
            GroupId = a.GroupId,
            OverridesTemplateId = a.OverridesTemplateId,
            Source = a.TenantId == null
                ? "platform"
                : a.OverridesTemplateId != null
                    ? "override"
                    : "custom"
        };
}