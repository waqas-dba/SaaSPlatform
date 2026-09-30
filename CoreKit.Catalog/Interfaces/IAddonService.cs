using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IAddonService
{
    Task<AddonGroupDto> CreateGroupAsync(Guid tenantId, CreateAddonGroupRequest request, CancellationToken ct = default);
    Task<AddonGroupDto> UpdateGroupAsync(Guid tenantId, Guid groupId, UpdateAddonGroupRequest request, CancellationToken ct = default);
    Task DeleteGroupAsync(Guid tenantId, Guid groupId, CancellationToken ct = default);
    Task<List<AddonGroupDto>> GetGroupsAsync(Guid tenantId, CancellationToken ct = default);
    Task<AddonGroupDto?> GetGroupByIdAsync(Guid tenantId, Guid groupId, CancellationToken ct = default);

    Task<AddonDto> AddAddonAsync(Guid tenantId, Guid groupId, AddAddonToGroupRequest request, CancellationToken ct = default);
    Task<AddonDto> UpdateAddonAsync(Guid tenantId, Guid groupId, Guid addonId, UpdateAddonRequest request, CancellationToken ct = default);
    Task RemoveAddonAsync(Guid tenantId, Guid groupId, Guid addonId, CancellationToken ct = default);
}