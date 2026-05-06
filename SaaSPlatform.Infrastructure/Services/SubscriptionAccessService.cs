using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.Billing.Interfaces;
using SaaSPlatform.Infrastructure.Persistence;

namespace SaaSPlatform.Infrastructure.Services;

public class SubscriptionAccessService : ISubscriptionAccessService
{
    private readonly SaaSPlatformDbContext _db;

    public SubscriptionAccessService(SaaSPlatformDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsTenantAllowedAsync(Guid tenantId)
    {
        var subscription = await _db.Subscriptions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync();

        if (subscription == null)
            return true;

        return subscription.Status is
            SubscriptionStatus.Active or
            SubscriptionStatus.Trialing or
            SubscriptionStatus.GracePeriod;
    }
}