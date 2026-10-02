using CoreKit.SharedKernel.Models;

namespace CoreKit.SharedKernel.Interfaces;

public interface IProductOrderInfoProvider
{
    /// <summary>
    /// Product as sold at one store. Returns null when the product does not
    /// exist, belongs to another tenant's menu, or is not assigned to the store.
    /// </summary>
    Task<ProductOrderInfo?> GetProductInfoAsync(
        Guid productId, Guid storeId, CancellationToken ct = default);

    /// <summary>Returns null unless the variant belongs to the given product.</summary>
    Task<ProductVariantOrderInfo?> GetVariantInfoAsync(
        Guid variantId, Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Resolves the selected add-ons from the database and validates them
    /// against the product's modifier-group rules (min/max per group).
    /// </summary>
    Task<AddonSelectionResult> ResolveAddonsAsync(
        Guid productId, IReadOnlyCollection<Guid> addonIds, CancellationToken ct = default);
}