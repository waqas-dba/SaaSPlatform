using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IVariantGroupService
{
    Task<List<VariantGroupDto>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<VariantGroupDto?> GetByIdAsync(Guid groupId, Guid tenantId, CancellationToken ct = default);
    Task<VariantGroupDto> CreateAsync(Guid tenantId, CreateVariantGroupRequest request, CancellationToken ct = default);
    Task<VariantGroupDto> UpdateAsync(Guid groupId, Guid tenantId, UpdateVariantGroupRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid groupId, Guid tenantId, CancellationToken ct = default);

    // Updated to include groupId for option operations
    Task<VariantGroupDto> AddOptionAsync(Guid groupId, Guid tenantId, VariantGroupOptionRequest request, CancellationToken ct = default);
    Task<VariantGroupDto> UpdateOptionAsync(Guid groupId, Guid optionId, Guid tenantId, VariantGroupOptionRequest request, CancellationToken ct = default);
    Task<VariantGroupDto> RemoveOptionAsync(Guid groupId, Guid optionId, Guid tenantId, CancellationToken ct = default);
}