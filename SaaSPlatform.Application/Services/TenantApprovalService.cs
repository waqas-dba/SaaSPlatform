using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Billing.Interfaces;
using SaaSPlatform.Core.Tenant.Enums;
using SaaSPlatform.Core.Billing.Enums;

namespace SaaSPlatform.Application.Services;

public class TenantApprovalService
{
    private readonly ITenantAccountRepository _tenantRepo;
    private readonly ISubscriptionRepository _subRepo;

    public TenantApprovalService(ITenantAccountRepository tenantRepo, ISubscriptionRepository subRepo)
    {
        _tenantRepo = tenantRepo;
        _subRepo = subRepo;
    }

    public async Task<(bool Success, string Message)> ApproveAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId);
        if (tenant is null) return (false, "Tenant not found.");
        if (tenant.RegistrationStatus == RegistrationStatus.Approved)
            return (false, "Tenant is already approved.");

        tenant.RegistrationStatus = RegistrationStatus.Approved;
        tenant.IsActive = true;
        _tenantRepo.Update(tenant);

        var subscription = await _subRepo.GetActiveByTenantIdAsync(tenantId);
        if (subscription is not null && subscription.Status == SubscriptionStatus.Trialing)
        {
            subscription.Status = SubscriptionStatus.Active;
            subscription.TrialEndsAt = DateTime.UtcNow.AddDays(14);
            subscription.NextBillingDate = DateTime.UtcNow.AddDays(14);
            _subRepo.Update(subscription);
        }

        return (true, "Tenant approved successfully.");
    }

    public async Task<(bool Success, string Message)> RejectAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId);
        if (tenant is null) return (false, "Tenant not found.");
        tenant.RegistrationStatus = RegistrationStatus.Rejected;
        _tenantRepo.Update(tenant);
        return (true, "Tenant rejected.");
    }
}