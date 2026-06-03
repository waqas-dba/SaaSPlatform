namespace CoreKit.Catalog.Interfaces;

public interface IAddonGroupRepository
{
    Task<bool> ExistsForTenantAsync(Guid id, Guid tenantId, CancellationToken ct = default);
}