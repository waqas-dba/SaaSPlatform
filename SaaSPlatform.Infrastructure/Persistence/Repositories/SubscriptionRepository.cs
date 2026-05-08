using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly SaaSPlatformDbContext _db;
    public SubscriptionRepository(SaaSPlatformDbContext db) => _db = db;

    public void Add(Subscription s) => _db.Subscriptions.Add(s);
    public void Update(Subscription s) => _db.Subscriptions.Update(s);
    public async Task<Subscription?> GetActiveByTenantIdAsync(Guid tenantId, CancellationToken ct)
        => await _db.Subscriptions.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
    public async Task<Plan?> GetPlanByNameAsync(string planName, CancellationToken ct)
        => await _db.Plans.FirstOrDefaultAsync(p => p.Name == planName, ct);
}