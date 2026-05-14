namespace CoreKit.Tenant.Abstractions;

public interface IStoreLimitService
{
    Task<int?> GetMaxStoresAsync(Guid tenantId, CancellationToken ct = default);
}