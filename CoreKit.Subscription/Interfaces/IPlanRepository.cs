using CoreKit.Subscription.Entities;

namespace CoreKit.Subscription.Interfaces;

public interface IPlanRepository
{
    Task<List<Plan>> GetAllAsync(CancellationToken ct = default);
    Task<Plan?> GetByIdAsync(Guid planId, CancellationToken ct = default);
    void Add(Plan plan);
    void Update(Plan plan);
}