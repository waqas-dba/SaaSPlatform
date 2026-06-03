// CoreKit.Catalog/Services/AddonService.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreKit.Catalog.Services;

public class AddonService : IAddonService
{
    private readonly CatalogDbContext _db;
    private readonly IPlanLimitProvider? _planLimit;
    private readonly ILogger<AddonService> _logger;

    public AddonService(
        CatalogDbContext db,
        ILogger<AddonService> logger,
        IPlanLimitProvider? planLimit = null)
    {
        _db = db;
        _logger = logger;
        _planLimit = planLimit;

        if (_planLimit == null)
            _logger.LogWarning(
                "IPlanLimitProvider not registered. " +
                "Add-on limits will not be enforced.");
    }

    // ── Groups ───────────────────────────────────────────────────────

    public async Task<AddonGroupDto> CreateGroupAsync(
        Guid tenantId,
        CreateAddonGroupRequest request,
        CancellationToken ct = default)
    {
        if (_planLimit != null &&
            !await _planLimit.IsAddonsEnabledAsync(tenantId, ct))
            throw new ForbiddenException(
                "Your plan does not include add-ons.");

        var group = new AddonGroup
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            TenantId = tenantId
        };

        foreach (var item in request.Addons)
        {
            group.Addons.Add(new Addon
            {
                Id = Guid.NewGuid(),
                Name = item.Name.Trim(),
                AdditionalPrice = item.AdditionalPrice,
                IsActive = true
            });
        }

        _db.AddonGroups.Add(group);
        await _db.SaveChangesAsync(ct);

        return MapGroupToDto(group);
    }

    public async Task<AddonGroupDto> UpdateGroupAsync(
        Guid groupId,
        Guid tenantId,
        UpdateAddonGroupRequest request,
        CancellationToken ct = default)
    {
        var group = await _db.AddonGroups
            .Include(g => g.Addons)
            .FirstOrDefaultAsync(
                g => g.Id == groupId && g.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException("Addon group not found.");

        if (request.Name is not null)
            group.Name = request.Name.Trim();

        await _db.SaveChangesAsync(ct);
        return MapGroupToDto(group);
    }

    public async Task DeleteGroupAsync(
        Guid groupId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var group = await _db.AddonGroups
            .Include(g => g.Addons)
            .FirstOrDefaultAsync(
                g => g.Id == groupId && g.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException("Addon group not found.");

        // Detach group from any products that reference it
        await _db.Products
            .Where(p => p.AddonGroupId == groupId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.AddonGroupId, (Guid?)null), ct);

        _db.AddonGroups.Remove(group);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<AddonGroupDto>> GetGroupsByTenantAsync(
        Guid tenantId,
        CancellationToken ct = default)
    {
        var groups = await _db.AddonGroups
            .Include(g => g.Addons)
            .Where(g => g.TenantId == tenantId)
            .OrderBy(g => g.Name)
            .ToListAsync(ct);

        return groups.Select(MapGroupToDto).ToList();
    }

    public async Task<AddonGroupDto?> GetGroupByIdAsync(
        Guid groupId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var group = await _db.AddonGroups
            .Include(g => g.Addons)
            .FirstOrDefaultAsync(
                g => g.Id == groupId && g.TenantId == tenantId, ct);

        return group is null ? null : MapGroupToDto(group);
    }

    // ── Addons inside a group ────────────────────────────────────────

    public async Task<AddonDto> AddToGroupAsync(
        Guid groupId,
        Guid tenantId,
        AddAddonToGroupRequest request,
        CancellationToken ct = default)
    {
        var group = await _db.AddonGroups
            .Include(g => g.Addons)
            .FirstOrDefaultAsync(
                g => g.Id == groupId && g.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException("Addon group not found.");

        if (_planLimit != null)
        {
            var max = await _planLimit
                .GetMaxAddonsPerProductAsync(tenantId, ct);

            if (max.HasValue && group.Addons.Count >= max.Value)
                throw new InvalidOperationException(
                    $"This group already has the maximum of " +
                    $"{max.Value} add-ons allowed by your plan.");
        }

        var addon = new Addon
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            AdditionalPrice = request.AdditionalPrice,
            AddonGroupId = groupId,
            IsActive = true
        };

        _db.Addons.Add(addon);
        await _db.SaveChangesAsync(ct);

        return MapAddonToDto(addon);
    }

    public async Task<AddonDto> UpdateAddonAsync(
        Guid addonId,
        Guid tenantId,
        AddAddonToGroupRequest request,
        CancellationToken ct = default)
    {
        // Verify ownership via the group
        var addon = await _db.Addons
            .Include(a => a.AddonGroup)
            .FirstOrDefaultAsync(a => a.Id == addonId, ct)
            ?? throw new KeyNotFoundException("Addon not found.");

        if (addon.AddonGroup?.TenantId != tenantId)
            throw new ForbiddenException(
                "You do not have access to this addon.");

        addon.Name = request.Name.Trim();
        addon.AdditionalPrice = request.AdditionalPrice;

        await _db.SaveChangesAsync(ct);
        return MapAddonToDto(addon);
    }

    public async Task RemoveFromGroupAsync(
        Guid addonId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var addon = await _db.Addons
            .Include(a => a.AddonGroup)
            .FirstOrDefaultAsync(a => a.Id == addonId, ct)
            ?? throw new KeyNotFoundException("Addon not found.");

        if (addon.AddonGroup?.TenantId != tenantId)
            throw new ForbiddenException(
                "You do not have access to this addon.");

        _db.Addons.Remove(addon);
        await _db.SaveChangesAsync(ct);
    }

    // ── Mappers ──────────────────────────────────────────────────────

    private static AddonGroupDto MapGroupToDto(AddonGroup g) => new()
    {
        Id = g.Id,
        Name = g.Name,
        Addons = g.Addons
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .Select(MapAddonToDto)
            .ToList()
    };

    private static AddonDto MapAddonToDto(Addon a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        AdditionalPrice = a.AdditionalPrice
    };
}