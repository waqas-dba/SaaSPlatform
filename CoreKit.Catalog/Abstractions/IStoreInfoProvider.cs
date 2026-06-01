// CoreKit.Catalog/Abstractions/IStoreInfoProvider.cs
namespace CoreKit.Catalog.Abstractions;

/// <summary>
/// Provides minimal store information needed by the Catalog module
/// without taking a hard dependency on CoreKit.Tenant entities or DbContext.
/// Implement this in the host application (or Tenant module) and register
/// it via AddCatalogModule so Catalog never queries Tenant tables directly.
/// </summary>
public interface IStoreInfoProvider
{
    /// <summary>
    /// Returns lightweight store info for plan-limit and variant-attribute
    /// resolution. Returns null if the store does not exist.
    /// </summary>
    Task<StoreInfo?> GetStoreInfoAsync(Guid storeId, CancellationToken ct = default);
}

/// <summary>
/// Lightweight projection of a Store — only the fields Catalog needs.
/// </summary>
public sealed class StoreInfo
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }

    // The store-type code is required to resolve VariantAttributeTemplates,
    // which are scoped per store type (e.g. "restaurant", "clothing").
    public string StoreTypeCode { get; init; } = default!;
}