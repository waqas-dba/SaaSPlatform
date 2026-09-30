namespace CoreKit.Catalog.Constants;

/// <summary>
/// Permission strings for the Catalog module. Keep in sync with
/// CoreKit.IAM Permissions.Catalog (the source the PermissionSeeder reads).
/// </summary>
public static class CatalogPermissions
{
    // Products (also cover variants, images and add-on groups)
    public const string ProductsView = "catalog.products.view";
    public const string ProductsCreate = "catalog.products.create";
    public const string ProductsUpdate = "catalog.products.update";
    public const string ProductsDelete = "catalog.products.delete";

    // Categories
    public const string CategoriesView = "catalog.categories.view";
    public const string CategoriesCreate = "catalog.categories.create";
    public const string CategoriesUpdate = "catalog.categories.update";
    public const string CategoriesDelete = "catalog.categories.delete";

    // Store menus (which products a store sells, store prices, copy/move menu)
    public const string MenuView = "catalog.menu.view";
    public const string MenuManage = "catalog.menu.manage";
}