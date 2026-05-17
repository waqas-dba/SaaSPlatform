using CoreKit.Tenant.Entities;

namespace CoreKit.Tenant.Interfaces;

public interface ITenantRepository
{
    /// <summary>Returns the tenant if it exists and is not soft-deleted.</summary>
    Task<TenantEntity?> GetByIdAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>Returns the tenant regardless of soft-delete status. Use for admin ops.</summary>
    Task<TenantEntity?> GetByIdIncludeDeletedAsync(Guid tenantId, CancellationToken ct = default);

    Task<List<TenantEntity>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    void Add(TenantEntity tenant);
    void Update(TenantEntity tenant);
}