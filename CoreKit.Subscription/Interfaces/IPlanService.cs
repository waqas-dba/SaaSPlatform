using CoreKit.Subscription.Entities;

namespace CoreKit.Subscription.Interfaces;

public interface IPlanService
{
    Task<List<Plan>> GetAllAsync();
    Task<Plan> CreateAsync(string name, string code, string? description,
        decimal monthlyPrice, decimal yearlyPrice,
        int? maxStores, int? maxProducts, int? maxCategories,
        bool customDomain, bool themeCustomization, int sortOrder);
}