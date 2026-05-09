using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.Billing.Interfaces;
using SaaSPlatform.Core.Tenant.Enums;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.SharedKernel.Interfaces;

public class TenantApprovalService
{
    private readonly ITenantAccountRepository _tenantRepo;
    private readonly ISubscriptionRepository _subRepo;
    private readonly IUnitOfWork _unitOfWork;

    public TenantApprovalService(
        ITenantAccountRepository tenantRepo,
        ISubscriptionRepository subRepo,
        IUnitOfWork unitOfWork)
    {
        _tenantRepo = tenantRepo;
        _subRepo = subRepo;
        _unitOfWork = unitOfWork;
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

        await _unitOfWork.SaveChangesAsync();  // <-- ADD THIS
        return (true, "Tenant approved successfully.");
    }

    public async Task<(bool Success, string Message)> RejectAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId);
        if (tenant is null) return (false, "Tenant not found.");
        tenant.RegistrationStatus = RegistrationStatus.Rejected;
        _tenantRepo.Update(tenant);
        await _unitOfWork.SaveChangesAsync();  // <-- ADD THIS
        return (true, "Tenant rejected.");
    }
}