using Microsoft.EntityFrameworkCore;
using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Interfaces;
using CoreKit.Subscription.Persistence;

namespace CoreKit.Subscription.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly SubscriptionDbContext _db;
    public PlanRepository(SubscriptionDbContext db) => _db = db;

    public async Task<List<Plan>> GetAllAsync(CancellationToken ct = default)
        => await _db.Plans.AsNoTracking().OrderBy(p => p.SortOrder).ToListAsync(ct);

    public async Task<Plan?> GetByIdAsync(Guid planId, CancellationToken ct = default)
        => await _db.Plans.FirstOrDefaultAsync(p => p.Id == planId, ct);

    public void Add(Plan plan) => _db.Plans.Add(plan);
    public void Update(Plan plan) => _db.Plans.Update(plan);
}