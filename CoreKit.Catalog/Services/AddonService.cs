using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Interfaces;

namespace CoreKit.Catalog.Services;

public sealed class AddonService : IAddonService
{
    private readonly IAddonGroupRepository _repo;
    private readonly IPlanLimitProvider? _planLimit;

    public AddonService(IAddonGroupRepository repo, IPlanLimitProvider? planLimit = null)
    {
        _repo = repo;
        _planLimit = planLimit;
    }

    public async Task<AddonGroupDto> CreateGroupAsync(
        Guid tenantId, CreateAddonGroupRequest request, CancellationToken ct = default)
    {
        await CatalogGuards.EnforceAddonsEnabledAsync(_planLimit, tenantId, ct);

        var name = ValidateName(request.Name);
        var activeCount = request.Addons.Count;
        ValidateSelectionRule(request.MinSelect, request.MaxSelect, activeCount);

        if (await _repo.NameExistsAsync(tenantId, name, excludeId: null, ct))
            throw new InvalidOperationException($"An add-on group named '{name}' already exists.");

        var group = new AddonGroup
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            MinSelect = request.MinSelect,
            MaxSelect = request.MaxSelect,
            SortOrder = request.SortOrder
        };

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in request.Addons)
        {
            var addonName = ValidateName(item.Name);
            ValidatePrice(item.AdditionalPrice);
            if (!seen.Add(addonName))
                throw new ArgumentException($"Duplicate add-on '{addonName}' in the same group.");

            group.Addons.Add(new Addon
            {
                Id = Guid.NewGuid(),
                Name = addonName,
                AdditionalPrice = item.AdditionalPrice,
                SortOrder = item.SortOrder,
                IsActive = true
            });
        }

        _repo.Add(group);
        await _repo.SaveChangesAsync(ct);

        return CatalogMapper.ToDto(group, includeInactive: true);
    }

    public async Task<AddonGroupDto> UpdateGroupAsync(
        Guid tenantId, Guid groupId, UpdateAddonGroupRequest request, CancellationToken ct = default)
    {
        var group = await LoadGroupAsync(tenantId, groupId, track: true, ct);

        if (request.Name is not null)
        {
            var name = ValidateName(request.Name);
            if (!string.Equals(name, group.Name, StringComparison.Ordinal) &&
                await _repo.NameExistsAsync(tenantId, name, group.Id, ct))
                throw new InvalidOperationException($"An add-on group named '{name}' already exists.");

            group.Name = name;
        }

        var min = request.MinSelect ?? group.MinSelect;
        var max = request.MaxSelect ?? group.MaxSelect;
        ValidateSelectionRule(min, max, group.Addons.Count(a => a.IsActive));

        group.MinSelect = min;
        group.MaxSelect = max;
        if (request.SortOrder.HasValue) group.SortOrder = request.SortOrder.Value;

        await _repo.SaveChangesAsync(ct);
        return CatalogMapper.ToDto(group, includeInactive: true);
    }

    public async Task DeleteGroupAsync(Guid tenantId, Guid groupId, CancellationToken ct = default)
    {
        var group = await LoadGroupAsync(tenantId, groupId, track: true, ct);

        if (await _repo.IsInUseAsync(groupId, ct))
            throw new InvalidOperationException(
                "This add-on group is still used by products. Remove it from those products first.");

        _repo.Remove(group);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task<List<AddonGroupDto>> GetGroupsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var groups = await _repo.GetByTenantAsync(tenantId, ct);
        return groups.Select(g => CatalogMapper.ToDto(g, includeInactive: true)).ToList();
    }

    public async Task<AddonGroupDto?> GetGroupByIdAsync(
        Guid tenantId, Guid groupId, CancellationToken ct = default)
    {
        var group = await _repo.GetByIdAsync(tenantId, groupId, track: false, ct);
        return group is null ? null : CatalogMapper.ToDto(group, includeInactive: true);
    }

    public async Task<AddonDto> AddAddonAsync(
        Guid tenantId, Guid groupId, AddAddonToGroupRequest request, CancellationToken ct = default)
    {
        var group = await LoadGroupAsync(tenantId, groupId, track: true, ct);

        var name = ValidateName(request.Name);
        ValidatePrice(request.AdditionalPrice);
        EnsureNameFree(group, name, exceptId: null);

        var addon = new Addon
        {
            Id = Guid.NewGuid(),
            AddonGroupId = group.Id,
            Name = name,
            AdditionalPrice = request.AdditionalPrice,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        _repo.AddAddon(addon);
        await _repo.SaveChangesAsync(ct);

        return CatalogMapper.ToDto(addon);
    }

    public async Task<AddonDto> UpdateAddonAsync(
        Guid tenantId, Guid groupId, Guid addonId, UpdateAddonRequest request, CancellationToken ct = default)
    {
        var group = await LoadGroupAsync(tenantId, groupId, track: true, ct);
        var addon = group.Addons.FirstOrDefault(a => a.Id == addonId)
            ?? throw new KeyNotFoundException("Add-on not found.");

        if (request.Name is not null)
        {
            var name = ValidateName(request.Name);
            EnsureNameFree(group, name, exceptId: addon.Id);
            addon.Name = name;
        }

        if (request.AdditionalPrice.HasValue)
        {
            ValidatePrice(request.AdditionalPrice.Value);
            addon.AdditionalPrice = request.AdditionalPrice.Value;
        }

        if (request.SortOrder.HasValue) addon.SortOrder = request.SortOrder.Value;

        if (request.IsActive.HasValue && request.IsActive.Value != addon.IsActive)
        {
            addon.IsActive = request.IsActive.Value;
            ValidateSelectionRule(group.MinSelect, group.MaxSelect, group.Addons.Count(a => a.IsActive));
        }

        await _repo.SaveChangesAsync(ct);
        return CatalogMapper.ToDto(addon);
    }

    public async Task RemoveAddonAsync(
        Guid tenantId, Guid groupId, Guid addonId, CancellationToken ct = default)
    {
        var group = await LoadGroupAsync(tenantId, groupId, track: true, ct);
        var addon = group.Addons.FirstOrDefault(a => a.Id == addonId)
            ?? throw new KeyNotFoundException("Add-on not found.");

        var remainingActive = group.Addons.Count(a => a.IsActive && a.Id != addonId);
        ValidateSelectionRule(group.MinSelect, group.MaxSelect, remainingActive);

        _repo.RemoveAddon(addon);
        await _repo.SaveChangesAsync(ct);
    }

    private async Task<AddonGroup> LoadGroupAsync(Guid tenantId, Guid groupId, bool track, CancellationToken ct)
        => await _repo.GetByIdAsync(tenantId, groupId, track, ct)
           ?? throw new KeyNotFoundException("Add-on group not found.");

    private static string ValidateName(string? raw)
    {
        var name = raw?.Trim() ?? string.Empty;
        if (name.Length == 0) throw new ArgumentException("Name is required.");
        if (name.Length > 200) throw new ArgumentException("Name is too long (max 200).");
        return name;
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0) throw new ArgumentException("Price cannot be negative.");
    }

    /// <summary>
    /// Min must be 0 or more, max at least 1, min not above max, and once the group has
    /// active options the customer must be able to satisfy the minimum.
    /// </summary>
    private static void ValidateSelectionRule(int min, int max, int activeAddonCount)
    {
        if (min < 0) throw new ArgumentException("MinSelect cannot be negative.");
        if (max < 1) throw new ArgumentException("MaxSelect must be at least 1.");
        if (min > max) throw new ArgumentException("MinSelect cannot be greater than MaxSelect.");

        if (activeAddonCount > 0 && min > activeAddonCount)
            throw new ArgumentException(
                $"MinSelect ({min}) is more than the number of active options ({activeAddonCount}).");
    }

    private static void EnsureNameFree(AddonGroup group, string name, Guid? exceptId)
    {
        var clash = group.Addons.Any(a =>
            a.Id != exceptId && string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));

        if (clash)
            throw new InvalidOperationException($"An option named '{name}' already exists in this group.");
    }
}