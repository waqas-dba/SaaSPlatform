using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Interfaces;
using CoreKit.Subscription.Persistence;

namespace CoreKit.Subscription.Services;

public class PlanService : IPlanService
{
    private readonly SubscriptionDbContext _db;
    private readonly IPlanRepository _planRepo;

    public PlanService(SubscriptionDbContext db, IPlanRepository planRepo)
    {
        _db = db;
        _planRepo = planRepo;
    }

    public async Task<List<Plan>> GetAllAsync() => await _planRepo.GetAllAsync();

    public async Task<Plan> CreateAsync(string name, string code, string? description,
        decimal monthlyPrice, decimal yearlyPrice,
        int? maxStores, int? maxProducts, int? maxCategories,
        bool customDomain, bool themeCustomization, int sortOrder)
    {
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Description = description,
            MonthlyPrice = monthlyPrice,
            YearlyPrice = yearlyPrice,
            MaxStores = maxStores,
            MaxProducts = maxProducts,
            MaxCategories = maxCategories,
            CustomDomainEnabled = customDomain,
            ThemeCustomizationEnabled = themeCustomization,
            SortOrder = sortOrder,
            IsActive = true
        };
        _planRepo.Add(plan);
        await _db.SaveChangesAsync();
        return plan;
    }
}