namespace CoreKit.Tenant.Services;

public interface IStoreLimitService
{
    Task<int?> GetMaxStoresAsync(Guid tenantId, CancellationToken ct = default);
}