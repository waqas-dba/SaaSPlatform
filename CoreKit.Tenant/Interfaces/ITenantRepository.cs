using CoreKit.Tenant.Entities;

namespace CoreKit.Tenant.Interfaces;

public interface ITenantRepository
{
    Task<TenantEntity?> GetByIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<List<TenantEntity>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

    void Add(TenantEntity tenant);
    void Update(TenantEntity tenant);
}