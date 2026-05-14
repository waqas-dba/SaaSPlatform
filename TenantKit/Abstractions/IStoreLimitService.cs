namespace TenantKit.Abstractions;

/// <summary>
/// Optional service to enforce store/branch limits (e.g. from a subscription plan).
/// </summary>
public interface IStoreLimitService
{
    Task<int?> GetMaxStoresAsync(Guid tenantId, CancellationToken ct = default);
}