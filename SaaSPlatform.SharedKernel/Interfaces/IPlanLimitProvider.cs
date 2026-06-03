using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.SharedKernel.Interfaces;

public interface IPlanLimitProvider
{
    Task<int?> GetMaxStoresAsync(Guid tenantId, CancellationToken ct = default);
    Task<int?> GetMaxProductsAsync(Guid tenantId, CancellationToken ct = default);
    Task<int?> GetMaxCategoriesAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> IsFeatureEnabledAsync(Guid tenantId, string featureCode, CancellationToken ct = default);

    Task<int> GetMaxCategoryLevelAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> IsVariantsEnabledAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> IsAddonsEnabledAsync(Guid tenantId, CancellationToken ct = default);
    Task<int?> GetMaxVariantsPerProductAsync(Guid tenantId, CancellationToken ct = default);
    Task<int?> GetMaxAddonsPerProductAsync(Guid tenantId, CancellationToken ct = default);
}