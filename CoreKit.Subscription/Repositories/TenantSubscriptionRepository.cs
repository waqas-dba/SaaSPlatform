using Microsoft.EntityFrameworkCore;
using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Interfaces;
using CoreKit.Subscription.Persistence;

namespace CoreKit.Subscription.Repositories;

public class TenantSubscriptionRepository : ITenantSubscriptionRepository
{
    private readonly SubscriptionDbContext _db;
    public TenantSubscriptionRepository(SubscriptionDbContext db) => _db = db;

    public async Task<TenantSubscription?> GetActiveSubscriptionAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.TenantSubscriptions
            .Include(ts => ts.Plan)
            .FirstOrDefaultAsync(ts => ts.TenantId == tenantId
                                       && ts.Status == SubscriptionStatus.Active
                                       && ts.StartDate <= DateTime.UtcNow
                                       && (ts.EndDate == null || ts.EndDate >= DateTime.UtcNow), ct);

    public void Add(TenantSubscription subscription) => _db.TenantSubscriptions.Add(subscription);
    public void Update(TenantSubscription subscription) => _db.TenantSubscriptions.Update(subscription);
}