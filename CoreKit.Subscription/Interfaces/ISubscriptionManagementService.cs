using CoreKit.Subscription.Entities;

namespace CoreKit.Subscription.Interfaces;

public interface ISubscriptionManagementService
{
    Task AssignSubscriptionAsync(Guid tenantId, Guid planId, DateTime? startDate = null, DateTime? endDate = null);
    Task<TenantSubscription?> GetActiveSubscriptionForTenantAsync(Guid tenantId);
}