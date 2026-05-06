namespace SaaSPlatform.Core.Billing.Interfaces;

public interface ISubscriptionAccessService
{
    Task<bool> IsTenantAllowedAsync(Guid tenantId);
}