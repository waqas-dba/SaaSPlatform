// CoreKit.Catalog/Constants/CatalogPermissions.cs
namespace CoreKit.Catalog.Constants;

/// <summary>
/// All permission strings for the Catalog module.
/// Register these in your IAM PermissionSeeder alongside the IAM permissions.
/// </summary>
public static class CatalogPermissions
{
    // ── Products ─────────────────────────────────────────────────────────
    public const string ProductsView = "catalog.products.view";
    public const string ProductsCreate = "catalog.products.create";
    public const string ProductsUpdate = "catalog.products.update";
    public const string ProductsDelete = "catalog.products.delete";

    // ── Categories ───────────────────────────────────────────────────────
    public const string CategoriesView = "catalog.categories.view";
    public const string CategoriesCreate = "catalog.categories.create";
    public const string CategoriesUpdate = "catalog.categories.update";
    public const string CategoriesDelete = "catalog.categories.delete";

    // ── Attribute Templates — Platform admin ─────────────────────────────
    /// <summary>
    /// Create / edit / delete platform-level base templates and groups.
    /// Intended for PlatformAdmin role only.
    /// </summary>
    public const string TemplatesManagePlatform = "catalog.templates.manage.platform";

    // ── Attribute Templates — Tenant admin ───────────────────────────────
    /// <summary>
    /// Create tenant-scoped overrides and custom attributes,
    /// assign templates to stores.
    /// </summary>
    public const string TemplatesManageTenant = "catalog.templates.manage.tenant";

    /// <summary>
    /// Assign / unassign templates to specific stores within the tenant.
    /// </summary>
    public const string TemplatesAssign = "catalog.templates.assign";

    // ── Attribute Templates — Store manager ──────────────────────────────
    /// <summary>
    /// Toggle IsRequired / IsVisible for attributes on a specific store.
    /// </summary>
    public const string TemplatesToggleStore = "catalog.templates.toggle.store";

    // ── View (all roles that can see the resolved template for a store) ──
    public const string TemplatesView = "catalog.templates.view";
}