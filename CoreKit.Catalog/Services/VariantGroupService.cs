using System.Text.Json;
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

public class VariantGroupService : IVariantGroupService
{
    private readonly CatalogDbContext _db;
    private readonly IPlanLimitProvider? _planLimit;

    public VariantGroupService(
        CatalogDbContext db,
        IPlanLimitProvider? planLimit = null)
    {
        _db = db;
        _planLimit = planLimit;
    }

    public async Task<List<VariantGroupDto>> GetByTenantAsync(
        Guid tenantId,
        CancellationToken ct = default)
    {
        var groups = await _db.VariantGroups
            .Include(g => g.Options)
            .ThenInclude(o => o.Template)
            .Where(g => g.TenantId == tenantId)
            .OrderBy(g => g.Name)
            .ToListAsync(ct);

        return groups.Select(MapToDto).ToList();
    }

    public async Task<VariantGroupDto?> GetByIdAsync(
        Guid groupId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var group = await FindAsync(groupId, tenantId, ct);
        return group is null ? null : MapToDto(group);
    }

    public async Task<VariantGroupDto> CreateAsync(
        Guid tenantId,
        CreateVariantGroupRequest request,
        CancellationToken ct = default)
    {
        if (_planLimit != null &&
            !await _planLimit.IsVariantsEnabledAsync(tenantId, ct))
            throw new ForbiddenException(
                "Your plan does not include product variants.");

        var duplicate = await _db.VariantGroups.AnyAsync(g =>
            g.TenantId == tenantId &&
            g.Name == request.Name.Trim(), ct);
        if (duplicate)
            throw new InvalidOperationException(
                "A variant group with this name already exists.");

        var group = new VariantGroup
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            StoreTypeCode = request.StoreTypeCode.Trim()
        };

        foreach (var opt in request.Options)
        {
            await ValidateOptionTemplateAsync(
                opt.TemplateId, request.StoreTypeCode, ct);

            group.Options.Add(new VariantGroupOption
            {
                Id = Guid.NewGuid(),
                TemplateId = opt.TemplateId,
                AllowedValuesJson = SerializeAllowedValues(opt.AllowedValues),
                SortOrder = opt.SortOrder
            });
        }

        _db.VariantGroups.Add(group);
        await _db.SaveChangesAsync(ct);
        return MapToDto(await FindAsync(group.Id, tenantId, ct)!);
    }

    public async Task<VariantGroupDto> UpdateAsync(
        Guid groupId,
        Guid tenantId,
        UpdateVariantGroupRequest request,
        CancellationToken ct = default)
    {
        var group = await FindAsync(groupId, tenantId, ct)
            ?? throw new KeyNotFoundException("Variant group not found.");

        if (request.Name is not null)
            group.Name = request.Name.Trim();

        await _db.SaveChangesAsync(ct);
        return MapToDto(group);
    }

    public async Task DeleteAsync(
        Guid groupId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var group = await FindAsync(groupId, tenantId, ct)
            ?? throw new KeyNotFoundException("Variant group not found.");

        await _db.Products
            .Where(p => p.VariantGroupId == groupId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.VariantGroupId, (Guid?)null), ct);

        _db.VariantGroups.Remove(group);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<VariantGroupDto> AddOptionAsync(
        Guid groupId,
        Guid tenantId,
        VariantGroupOptionRequest request,
        CancellationToken ct = default)
    {
        var group = await FindAsync(groupId, tenantId, ct)
            ?? throw new KeyNotFoundException("Variant group not found.");

        var alreadyHasTemplate = group.Options
            .Any(o => o.TemplateId == request.TemplateId);
        if (alreadyHasTemplate)
            throw new InvalidOperationException(
                "This template is already an option in the group.");

        await ValidateOptionTemplateAsync(
            request.TemplateId, group.StoreTypeCode, ct);

        group.Options.Add(new VariantGroupOption
        {
            Id = Guid.NewGuid(),
            VariantGroupId = groupId,
            TemplateId = request.TemplateId,
            AllowedValuesJson = SerializeAllowedValues(request.AllowedValues),
            SortOrder = request.SortOrder
        });

        await _db.SaveChangesAsync(ct);
        return MapToDto(group);
    }

    public async Task<VariantGroupDto> UpdateOptionAsync(
        Guid groupId,
        Guid optionId,
        Guid tenantId,
        VariantGroupOptionRequest request,
        CancellationToken ct = default)
    {
        var group = await FindAsync(groupId, tenantId, ct)
            ?? throw new KeyNotFoundException("Variant group not found.");

        var option = group.Options.FirstOrDefault(o => o.Id == optionId)
            ?? throw new KeyNotFoundException(
                "Option not found in this group.");

        option.AllowedValuesJson =
            SerializeAllowedValues(request.AllowedValues);
        option.SortOrder = request.SortOrder;

        await _db.SaveChangesAsync(ct);
        return MapToDto(group);
    }

    public async Task<VariantGroupDto> RemoveOptionAsync(
        Guid groupId,
        Guid optionId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var group = await FindAsync(groupId, tenantId, ct)
            ?? throw new KeyNotFoundException("Variant group not found.");

        var option = group.Options.FirstOrDefault(o => o.Id == optionId)
            ?? throw new KeyNotFoundException(
                "Option not found in this group.");

        group.Options.Remove(option);
        await _db.SaveChangesAsync(ct);
        return MapToDto(group);
    }

    private async Task<VariantGroup?> FindAsync(
        Guid groupId,
        Guid tenantId,
        CancellationToken ct)
        => await _db.VariantGroups
            .Include(g => g.Options)
            .ThenInclude(o => o.Template)
            .FirstOrDefaultAsync(
                g => g.Id == groupId && g.TenantId == tenantId, ct);

    private async Task ValidateOptionTemplateAsync(
        Guid templateId,
        string storeTypeCode,
        CancellationToken ct)
    {
        var exists = await _db.VariantAttributeTemplates
            .AnyAsync(t =>
                t.Id == templateId &&
                t.StoreTypeCode == storeTypeCode, ct);
        if (!exists)
            throw new InvalidOperationException(
                $"Variant template '{templateId}' does not exist " +
                $"for store type '{storeTypeCode}'.");
    }

    private static string? SerializeAllowedValues(List<string>? values)
        => values is null || values.Count == 0
            ? null
            : JsonSerializer.Serialize(values);

    private static List<string>? DeserializeAllowedValues(string? json)
        => string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<List<string>>(json);

    private static VariantGroupDto MapToDto(VariantGroup g) => new()
    {
        Id = g.Id,
        Name = g.Name,
        StoreTypeCode = g.StoreTypeCode,
        TenantId = g.TenantId,
        Options = g.Options
            .OrderBy(o => o.SortOrder)
            .Select(o => new VariantGroupOptionDto
            {
                Id = o.Id,
                TemplateId = o.TemplateId,
                TemplateName = o.Template.Name,
                AllowedValues = DeserializeAllowedValues(o.AllowedValuesJson),
                SortOrder = o.SortOrder
            }).ToList()
    };
}