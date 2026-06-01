// CoreKit.Catalog/Services/ProductAttributeTemplateService.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

public class ProductAttributeTemplateService : IProductAttributeTemplateService
{
    private readonly CatalogDbContext _db;

    public ProductAttributeTemplateService(CatalogDbContext db) => _db = db;

    // ── Platform admin ────────────────────────────────────────────────────

    public async Task<ProductAttributeTemplateDto> CreatePlatformTemplateAsync(
        CreateAttributeTemplateRequest request)
    {
        // Platform templates must have TenantId = null.
        if (request.TenantId != null)
            throw new InvalidOperationException(
                "Platform templates must not have a TenantId. " +
                "Use CreateTenantTemplateAsync for tenant-scoped attributes.");

        await EnsureUniqueNameAsync(request.StoreTypeCode, null, request.Name);

        var template = BuildTemplate(request, tenantId: null, overridesId: null);
        _db.AttributeTemplates.Add(template);
        await _db.SaveChangesAsync();

        return await MapToDtoAsync(template);
    }

    public async Task UpdatePlatformTemplateAsync(
        Guid templateId,
        UpdateAttributeTemplateRequest request)
    {
        var template = await _db.AttributeTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId && t.TenantId == null)
            ?? throw new KeyNotFoundException("Platform template not found.");

        ApplyUpdate(template, request);
        await _db.SaveChangesAsync();
    }

    public async Task DeletePlatformTemplateAsync(Guid templateId)
    {
        var template = await _db.AttributeTemplates
            .Include(t => t.StoreOverrides)
            .FirstOrDefaultAsync(t => t.Id == templateId && t.TenantId == null)
            ?? throw new KeyNotFoundException("Platform template not found.");

        // Remove all tenant overrides that point to this platform template.
        var tenantOverrides = await _db.AttributeTemplates
            .Where(t => t.OverridesTemplateId == templateId)
            .ToListAsync();

        _db.AttributeTemplates.RemoveRange(tenantOverrides);
        _db.AttributeTemplates.Remove(template);
        await _db.SaveChangesAsync();
    }

    // ── Tenant admin ──────────────────────────────────────────────────────

    public async Task<ProductAttributeTemplateDto> OverridePlatformTemplateAsync(
        OverrideAttributeTemplateRequest request)
    {
        // Load the platform template being overridden.
        var platform = await _db.AttributeTemplates
            .FirstOrDefaultAsync(t =>
                t.Id == request.PlatformTemplateId &&
                t.TenantId == null)
            ?? throw new KeyNotFoundException("Platform template not found.");

        // A tenant can only have one override per platform template.
        var existingOverride = await _db.AttributeTemplates
            .FirstOrDefaultAsync(t =>
                t.OverridesTemplateId == request.PlatformTemplateId &&
                t.TenantId == request.TenantId);

        if (existingOverride != null)
            throw new InvalidOperationException(
                "An override for this platform template already exists. " +
                "Use UpdateTenantTemplateAsync to modify it.");

        // Build the override row, copying from platform then applying deltas.
        var overrideTemplate = new ProductAttributeTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name ?? platform.Name,
            StoreTypeCode = platform.StoreTypeCode,
            FieldType = platform.FieldType,
            OptionsJson = request.OptionsJson ?? platform.OptionsJson,
            IsRequired = request.IsRequired ?? platform.IsRequired,
            IsVisible = request.IsVisible ?? platform.IsVisible,
            SortOrder = request.SortOrder ?? platform.SortOrder,
            TenantId = request.TenantId,
            OverridesTemplateId = platform.Id,
            GroupId = request.GroupId ?? platform.GroupId
        };

        _db.AttributeTemplates.Add(overrideTemplate);
        await _db.SaveChangesAsync();

        return await MapToDtoAsync(overrideTemplate);
    }

    public async Task<ProductAttributeTemplateDto> CreateTenantTemplateAsync(
        CreateAttributeTemplateRequest request)
    {
        if (request.TenantId == null)
            throw new InvalidOperationException(
                "TenantId is required for tenant-scoped templates.");

        await EnsureUniqueNameAsync(
            request.StoreTypeCode, request.TenantId, request.Name);

        var template = BuildTemplate(request, request.TenantId, overridesId: null);
        _db.AttributeTemplates.Add(template);
        await _db.SaveChangesAsync();

        return await MapToDtoAsync(template);
    }

    public async Task UpdateTenantTemplateAsync(
        Guid templateId,
        Guid tenantId,
        UpdateAttributeTemplateRequest request)
    {
        var template = await _db.AttributeTemplates
            .FirstOrDefaultAsync(t =>
                t.Id == templateId &&
                t.TenantId == tenantId)
            ?? throw new KeyNotFoundException("Tenant template not found.");

        ApplyUpdate(template, request);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteTenantTemplateAsync(Guid templateId, Guid tenantId)
    {
        var template = await _db.AttributeTemplates
            .FirstOrDefaultAsync(t =>
                t.Id == templateId &&
                t.TenantId == tenantId)
            ?? throw new KeyNotFoundException("Tenant template not found.");

        _db.AttributeTemplates.Remove(template);
        await _db.SaveChangesAsync();
    }

    // ── Assignment ────────────────────────────────────────────────────────

    public async Task AssignTemplateAsync(AssignTemplateRequest request)
    {
        // Verify the template exists and is accessible to this tenant.
        var template = await _db.AttributeTemplates
            .FirstOrDefaultAsync(t =>
                t.Id == request.TemplateId &&
                (t.TenantId == null || t.TenantId == request.TenantId))
            ?? throw new KeyNotFoundException(
                "Template not found or not accessible to this tenant.");

        var alreadyAssigned = await _db.TenantTemplateAssignments
            .AnyAsync(a =>
                a.TenantId == request.TenantId &&
                a.StoreId == request.StoreId &&
                a.TemplateId == request.TemplateId);

        if (alreadyAssigned) return;

        _db.TenantTemplateAssignments.Add(new TenantTemplateAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            StoreId = request.StoreId,
            TemplateId = request.TemplateId,
            StoreTypeCode = request.StoreTypeCode
        });

        await _db.SaveChangesAsync();
    }

    public async Task UnassignTemplateAsync(
        Guid tenantId,
        Guid? storeId,
        Guid templateId)
    {
        var assignment = await _db.TenantTemplateAssignments
            .FirstOrDefaultAsync(a =>
                a.TenantId == tenantId &&
                a.StoreId == storeId &&
                a.TemplateId == templateId)
            ?? throw new KeyNotFoundException("Assignment not found.");

        _db.TenantTemplateAssignments.Remove(assignment);
        await _db.SaveChangesAsync();
    }

    // ── Store-level toggle ────────────────────────────────────────────────

    public async Task SetStoreAttributeOverrideAsync(
        StoreAttributeOverrideRequest request)
    {
        var existing = await _db.StoreAttributeOverrides
            .FirstOrDefaultAsync(o =>
                o.StoreId == request.StoreId &&
                o.TemplateId == request.TemplateId);

        if (existing != null)
        {
            if (request.IsRequired.HasValue)
                existing.IsRequired = request.IsRequired;
            if (request.IsVisible.HasValue)
                existing.IsVisible = request.IsVisible;
        }
        else
        {
            _db.StoreAttributeOverrides.Add(new StoreAttributeOverride
            {
                Id = Guid.NewGuid(),
                StoreId = request.StoreId,
                TemplateId = request.TemplateId,
                IsRequired = request.IsRequired,
                IsVisible = request.IsVisible
            });
        }

        await _db.SaveChangesAsync();
    }

    // ── Queries ───────────────────────────────────────────────────────────

    public async Task<List<ProductAttributeGroupDto>> GetPlatformTemplatesAsync(
        string storeTypeCode)
    {
        // Load all platform templates for the store type, including ungrouped.
        var templates = await _db.AttributeTemplates
            .Where(t =>
                t.StoreTypeCode == storeTypeCode &&
                t.TenantId == null)
            .Include(t => t.Group)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

        return GroupTemplates(templates);
    }

    public async Task<List<ProductAttributeTemplateDto>> GetTenantTemplatesAsync(
        Guid tenantId,
        string storeTypeCode)
    {
        return await _db.AttributeTemplates
            .Where(t =>
                t.StoreTypeCode == storeTypeCode &&
                t.TenantId == tenantId)
            .Include(t => t.Group)
            .OrderBy(t => t.SortOrder)
            .Select(t => new ProductAttributeTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                StoreTypeCode = t.StoreTypeCode,
                FieldType = t.FieldType,
                OptionsJson = t.OptionsJson,
                IsRequired = t.IsRequired,
                IsVisible = t.IsVisible,
                SortOrder = t.SortOrder,
                TenantId = t.TenantId,
                GroupId = t.GroupId,
                GroupName = t.Group != null ? t.Group.Name : null,
                OverridesTemplateId = t.OverridesTemplateId,
                Source = t.OverridesTemplateId != null ? "override" : "custom"
            })
            .ToListAsync();
    }

    public async Task<List<ResolvedAttributeDto>> GetResolvedAttributesAsync(
        Guid storeId,
        Guid tenantId,
        string storeTypeCode)
    {
        // Step 1 — Load all platform base templates for this store type.
        var platformTemplates = await _db.AttributeTemplates
            .Where(t =>
                t.StoreTypeCode == storeTypeCode &&
                t.TenantId == null)
            .Include(t => t.Group)
            .ToListAsync();

        // Step 2 — Load all tenant overrides and custom attributes.
        var tenantTemplates = await _db.AttributeTemplates
            .Where(t =>
                t.StoreTypeCode == storeTypeCode &&
                t.TenantId == tenantId)
            .Include(t => t.Group)
            .ToListAsync();

        // Step 3 — Load store-level toggles for this store.
        var storeOverrides = await _db.StoreAttributeOverrides
            .Where(o => o.StoreId == storeId)
            .ToListAsync();

        // Step 4 — Load what the tenant has explicitly assigned to this store
        // (store-specific) or tenant-wide (StoreId = null).
        var assignments = await _db.TenantTemplateAssignments
            .Where(a =>
                a.TenantId == tenantId &&
                a.StoreTypeCode == storeTypeCode &&
                (a.StoreId == storeId || a.StoreId == null))
            .ToListAsync();

        var assignedTemplateIds = assignments
            .Select(a => a.TemplateId)
            .ToHashSet();

        // Step 5 — Build the effective attribute list.
        //
        // For each platform template:
        //   a) Check if there is a tenant override → use override values.
        //   b) Check if there is a store-level toggle → apply on top.
        //   c) If the tenant has explicit assignments and this template is not
        //      in the assigned set, skip it (tenant opted out).
        //
        // Then append tenant-custom attributes (no platform equivalent)
        // that are in the assignment set or if there are no assignments at all
        // (meaning the tenant hasn't configured anything and wants all defaults).

        var hasExplicitAssignments = assignedTemplateIds.Count > 0;

        // Index tenant templates by what they override for O(1) lookup.
        var overrideByPlatformId = tenantTemplates
            .Where(t => t.OverridesTemplateId != null)
            .ToDictionary(t => t.OverridesTemplateId!.Value);

        var storeOverrideByTemplateId = storeOverrides
            .ToDictionary(o => o.TemplateId);

        var resolved = new List<ResolvedAttributeDto>();

        foreach (var platform in platformTemplates)
        {
            // Determine effective template row (platform or tenant override).
            var effectiveRow = overrideByPlatformId.TryGetValue(
                platform.Id, out var tenantOverride)
                ? tenantOverride
                : platform;

            // If tenant has explicit assignments and this template
            // (using the effective row id) is not assigned, skip it.
            if (hasExplicitAssignments &&
                !assignedTemplateIds.Contains(effectiveRow.Id) &&
                !assignedTemplateIds.Contains(platform.Id))
                continue;

            // Apply store-level toggle on top.
            var storeToggle = storeOverrideByTemplateId.TryGetValue(
                effectiveRow.Id, out var toggleRow)
                ? toggleRow
                : null;

            resolved.Add(new ResolvedAttributeDto
            {
                TemplateId = effectiveRow.Id,
                Name = effectiveRow.Name,
                StoreTypeCode = storeTypeCode,
                FieldType = effectiveRow.FieldType,
                OptionsJson = effectiveRow.OptionsJson,
                IsRequired = storeToggle?.IsRequired ?? effectiveRow.IsRequired,
                IsVisible = storeToggle?.IsVisible ?? effectiveRow.IsVisible,
                SortOrder = effectiveRow.SortOrder,
                GroupId = effectiveRow.GroupId,
                GroupName = effectiveRow.Group?.Name,
                Source = tenantOverride != null
                    ? "tenant-override"
                    : "platform"
            });
        }

        // Append tenant-custom attributes (OverridesTemplateId = null).
        var customAttributes = tenantTemplates
            .Where(t => t.OverridesTemplateId == null);

        foreach (var custom in customAttributes)
        {
            if (hasExplicitAssignments &&
                !assignedTemplateIds.Contains(custom.Id))
                continue;

            var storeToggle = storeOverrideByTemplateId.TryGetValue(
                custom.Id, out var toggleRow)
                ? toggleRow
                : null;

            resolved.Add(new ResolvedAttributeDto
            {
                TemplateId = custom.Id,
                Name = custom.Name,
                StoreTypeCode = storeTypeCode,
                FieldType = custom.FieldType,
                OptionsJson = custom.OptionsJson,
                IsRequired = storeToggle?.IsRequired ?? custom.IsRequired,
                IsVisible = storeToggle?.IsVisible ?? custom.IsVisible,
                SortOrder = custom.SortOrder,
                GroupId = custom.GroupId,
                GroupName = custom.Group?.Name,
                Source = "tenant-custom"
            });
        }

        return resolved
            .OrderBy(r => r.GroupId == null ? 1 : 0) // grouped first
            .ThenBy(r => r.GroupName)
            .ThenBy(r => r.SortOrder)
            .ToList();
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private async Task EnsureUniqueNameAsync(
        string storeTypeCode,
        Guid? tenantId,
        string name)
    {
        var exists = await _db.AttributeTemplates.AnyAsync(t =>
            t.StoreTypeCode == storeTypeCode &&
            t.TenantId == tenantId &&
            t.Name == name);

        if (exists)
            throw new InvalidOperationException(
                $"An attribute named '{name}' already exists " +
                $"for store type '{storeTypeCode}' " +
                $"in this scope.");
    }

    private static ProductAttributeTemplate BuildTemplate(
        CreateAttributeTemplateRequest r,
        Guid? tenantId,
        Guid? overridesId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = r.Name,
            StoreTypeCode = r.StoreTypeCode,
            FieldType = r.FieldType,
            OptionsJson = r.OptionsJson,
            IsRequired = r.IsRequired,
            IsVisible = r.IsVisible,
            SortOrder = r.SortOrder,
            TenantId = tenantId,
            OverridesTemplateId = overridesId,
            GroupId = r.GroupId
        };

    private static void ApplyUpdate(
        ProductAttributeTemplate t,
        UpdateAttributeTemplateRequest r)
    {
        if (r.Name != null) t.Name = r.Name;
        if (r.OptionsJson != null) t.OptionsJson = r.OptionsJson;
        if (r.IsRequired.HasValue) t.IsRequired = r.IsRequired.Value;
        if (r.IsVisible.HasValue) t.IsVisible = r.IsVisible.Value;
        if (r.SortOrder.HasValue) t.SortOrder = r.SortOrder.Value;
        if (r.GroupId.HasValue) t.GroupId = r.GroupId.Value;
    }

    private async Task<ProductAttributeTemplateDto> MapToDtoAsync(
        ProductAttributeTemplate t)
    {
        string? groupName = null;
        if (t.GroupId.HasValue)
        {
            groupName = await _db.AttributeGroups
                .Where(g => g.Id == t.GroupId.Value)
                .Select(g => g.Name)
                .FirstOrDefaultAsync();
        }

        return new ProductAttributeTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            StoreTypeCode = t.StoreTypeCode,
            FieldType = t.FieldType,
            OptionsJson = t.OptionsJson,
            IsRequired = t.IsRequired,
            IsVisible = t.IsVisible,
            SortOrder = t.SortOrder,
            TenantId = t.TenantId,
            GroupId = t.GroupId,
            GroupName = groupName,
            OverridesTemplateId = t.OverridesTemplateId,
            Source = t.TenantId == null
                ? "platform"
                : t.OverridesTemplateId != null
                    ? "override"
                    : "custom"
        };
    }

    private static List<ProductAttributeGroupDto> GroupTemplates(
        List<ProductAttributeTemplate> templates)
    {
        var groups = templates
            .Where(t => t.Group != null)
            .GroupBy(t => t.Group!)
            .Select(g => new ProductAttributeGroupDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                StoreTypeCode = g.Key.StoreTypeCode,
                SortOrder = g.Key.SortOrder,
                Attributes = g
                    .OrderBy(t => t.SortOrder)
                    .Select(t => new ProductAttributeTemplateDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        StoreTypeCode = t.StoreTypeCode,
                        FieldType = t.FieldType,
                        OptionsJson = t.OptionsJson,
                        IsRequired = t.IsRequired,
                        IsVisible = t.IsVisible,
                        SortOrder = t.SortOrder,
                        GroupId = t.GroupId,
                        GroupName = t.Group?.Name,
                        Source = "platform"
                    }).ToList()
            })
            .OrderBy(g => g.SortOrder)
            .ToList();

        // Ungrouped attributes in a synthetic group.
        var ungrouped = templates
            .Where(t => t.GroupId == null)
            .OrderBy(t => t.SortOrder)
            .Select(t => new ProductAttributeTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                StoreTypeCode = t.StoreTypeCode,
                FieldType = t.FieldType,
                OptionsJson = t.OptionsJson,
                IsRequired = t.IsRequired,
                IsVisible = t.IsVisible,
                SortOrder = t.SortOrder,
                Source = "platform"
            }).ToList();

        if (ungrouped.Any())
        {
            groups.Add(new ProductAttributeGroupDto
            {
                Id = Guid.Empty,
                Name = "General",
                SortOrder = int.MaxValue,
                Attributes = ungrouped
            });
        }

        return groups;
    }
}