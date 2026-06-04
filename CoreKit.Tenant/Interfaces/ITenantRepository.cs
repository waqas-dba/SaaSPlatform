using CoreKit.Tenant.Entities;

namespace CoreKit.Tenant.Interfaces;

public interface ITenantRepository
{
    Task<TenantEntity?> GetByIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantEntity?> GetByIdIncludeDeletedAsync(Guid tenantId, CancellationToken ct = default);
    Task<List<TenantEntity>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default);   // NEW
    void Add(TenantEntity tenant);
    void Update(TenantEntity tenant);
}