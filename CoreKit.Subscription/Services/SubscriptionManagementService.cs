using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Interfaces;
using CoreKit.Subscription.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CoreKit.Subscription.Services;

public class SubscriptionManagementService : ISubscriptionManagementService
{
    private readonly SubscriptionDbContext _db;
    private readonly ITenantSubscriptionRepository _subscriptionRepo;
    private readonly IMemoryCache _cache;

    public SubscriptionManagementService(
        SubscriptionDbContext db,
        ITenantSubscriptionRepository subscriptionRepo,
        IMemoryCache cache)
    {
        _db = db;
        _subscriptionRepo = subscriptionRepo;
        _cache = cache;
    }

    public async Task AssignSubscriptionAsync(
        Guid tenantId,
        Guid planId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var existingActive = await _db.TenantSubscriptions
            .Where(ts => ts.TenantId == tenantId && ts.Status == SubscriptionStatus.Active)
            .ToListAsync();

        foreach (var sub in existingActive)
        {
            sub.Status = SubscriptionStatus.Cancelled;
            sub.EndDate = DateTime.UtcNow;
        }

        var subscription = new TenantSubscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanId = planId,
            StartDate = startDate ?? DateTime.UtcNow,
            EndDate = endDate,
            Status = SubscriptionStatus.Active
        };

        _subscriptionRepo.Add(subscription);
        await _db.SaveChangesAsync();

        // Evict cached plan so new limits take effect immediately
        _cache.Remove($"plan:active:{tenantId}");
    }

    public async Task<TenantSubscription?> GetActiveSubscriptionForTenantAsync(Guid tenantId)
        => await _subscriptionRepo.GetActiveSubscriptionAsync(tenantId);
}