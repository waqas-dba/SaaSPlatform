// CoreKit.Catalog/Interfaces/IProductAttributeTemplateService.cs
using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IProductAttributeTemplateService
{
    // ── Platform admin operations ─────────────────────────────────────────

    /// <summary>Creates a platform-level base template (TenantId = null).</summary>
    Task<ProductAttributeTemplateDto> CreatePlatformTemplateAsync(
        CreateAttributeTemplateRequest request);

    /// <summary>Updates a platform-level template.</summary>
    Task UpdatePlatformTemplateAsync(
        Guid templateId,
        UpdateAttributeTemplateRequest request);

    /// <summary>Deletes a platform-level template and all tenant overrides of it.</summary>
    Task DeletePlatformTemplateAsync(Guid templateId);

    // ── Tenant admin operations ───────────────────────────────────────────

    /// <summary>
    /// Creates a tenant-scoped override of a platform template.
    /// The platform row is never modified.
    /// Only the fields provided in the request are changed; the rest
    /// are copied from the platform template.
    /// </summary>
    Task<ProductAttributeTemplateDto> OverridePlatformTemplateAsync(
        OverrideAttributeTemplateRequest request);

    /// <summary>
    /// Creates a net-new custom attribute for the tenant
    /// (no platform template equivalent).
    /// </summary>
    Task<ProductAttributeTemplateDto> CreateTenantTemplateAsync(
        CreateAttributeTemplateRequest request);

    /// <summary>Updates a tenant-scoped template or override.</summary>
    Task UpdateTenantTemplateAsync(
        Guid templateId,
        Guid tenantId,
        UpdateAttributeTemplateRequest request);

    /// <summary>Deletes a tenant-scoped template or override.</summary>
    Task DeleteTenantTemplateAsync(Guid templateId, Guid tenantId);

    // ── Assignment ────────────────────────────────────────────────────────

    /// <summary>
    /// Assigns a template to a store (or tenant-wide if StoreId is null).
    /// </summary>
    Task AssignTemplateAsync(AssignTemplateRequest request);

    /// <summary>Removes a template assignment.</summary>
    Task UnassignTemplateAsync(
        Guid tenantId,
        Guid? storeId,
        Guid templateId);

    // ── Store-level toggle ────────────────────────────────────────────────

    /// <summary>
    /// Store manager sets IsRequired / IsVisible for an attribute on
    /// their store. Creates the override row if it does not exist.
    /// </summary>
    Task SetStoreAttributeOverrideAsync(StoreAttributeOverrideRequest request);

    // ── Queries ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all platform base templates for a store type,
    /// grouped by their group (ungrouped attributes are in a null-group bucket).
    /// </summary>
    Task<List<ProductAttributeGroupDto>> GetPlatformTemplatesAsync(
        string storeTypeCode);

    /// <summary>
    /// Returns the effective (resolved) attribute list for a specific store,
    /// applying all three precedence layers in order:
    ///   1. Platform base template
    ///   2. Tenant override / custom attributes
    ///   3. Store-level toggles
    /// </summary>
    Task<List<ResolvedAttributeDto>> GetResolvedAttributesAsync(
        Guid storeId,
        Guid tenantId,
        string storeTypeCode);

    /// <summary>
    /// Returns the tenant's own templates (overrides + custom)
    /// for a given store type.
    /// </summary>
    Task<List<ProductAttributeTemplateDto>> GetTenantTemplatesAsync(
        Guid tenantId,
        string storeTypeCode);
}