// CoreKit.Catalog/Interfaces/IAddonService.cs
using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IAddonService
{
    // ── Group operations ────────────────────────────────────────────
    Task<AddonGroupDto> CreateGroupAsync(
        Guid tenantId,
        CreateAddonGroupRequest request,
        CancellationToken ct = default);

    Task<AddonGroupDto> UpdateGroupAsync(
        Guid groupId,
        Guid tenantId,
        UpdateAddonGroupRequest request,
        CancellationToken ct = default);

    Task DeleteGroupAsync(
        Guid groupId,
        Guid tenantId,
        CancellationToken ct = default);

    Task<List<AddonGroupDto>> GetGroupsByTenantAsync(
        Guid tenantId,
        CancellationToken ct = default);

    Task<AddonGroupDto?> GetGroupByIdAsync(
        Guid groupId,
        Guid tenantId,
        CancellationToken ct = default);

    // ── Addon operations inside a group ────────────────────────────
    Task<AddonDto> AddToGroupAsync(
        Guid groupId,
        Guid tenantId,
        AddAddonToGroupRequest request,
        CancellationToken ct = default);

    Task<AddonDto> UpdateAddonAsync(
        Guid addonId,
        Guid tenantId,
        AddAddonToGroupRequest request,
        CancellationToken ct = default);

    Task RemoveFromGroupAsync(
        Guid addonId,
        Guid tenantId,
        CancellationToken ct = default);
}