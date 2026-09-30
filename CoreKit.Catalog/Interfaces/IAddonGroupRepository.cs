using CoreKit.Catalog.Entities;

namespace CoreKit.Catalog.Interfaces;

public interface IAddonGroupRepository
{
    Task<AddonGroup?> GetByIdAsync(Guid tenantId, Guid id, bool track, CancellationToken ct = default);
    Task<List<AddonGroup>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<int> CountExistingAsync(Guid tenantId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId, CancellationToken ct = default);
    Task<bool> IsInUseAsync(Guid groupId, CancellationToken ct = default);

    void Add(AddonGroup group);
    void Remove(AddonGroup group);
    void AddAddon(Addon addon);
    void RemoveAddon(Addon addon);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}