using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IAddonService
{
    Task<AddonGroupDto> CreateGroupAsync(Guid tenantId, CreateAddonGroupRequest request, CancellationToken ct = default);
    Task<AddonGroupDto> UpdateGroupAsync(Guid groupId, Guid tenantId, UpdateAddonGroupRequest request, CancellationToken ct = default);
    Task DeleteGroupAsync(Guid groupId, Guid tenantId, CancellationToken ct = default);
    Task<List<AddonGroupDto>> GetGroupsByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<AddonGroupDto?> GetGroupByIdAsync(Guid groupId, Guid tenantId, CancellationToken ct = default);

    // Updated to include groupId for addon operations
    Task<AddonDto> AddToGroupAsync(Guid groupId, Guid tenantId, AddAddonToGroupRequest request, CancellationToken ct = default);
    Task<AddonDto> UpdateAddonAsync(Guid groupId, Guid addonId, Guid tenantId, AddAddonToGroupRequest request, CancellationToken ct = default);
    Task RemoveFromGroupAsync(Guid groupId, Guid addonId, Guid tenantId, CancellationToken ct = default);
}