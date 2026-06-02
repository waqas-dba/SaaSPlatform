using System.Reflection;

namespace CoreKit.IAM.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string Create = "users.create";
        public const string Update = "users.update";
        public const string Delete = "users.delete";
        public const string View = "users.view";
        public const string AssignRole = "users.assign_role";
        public const string RemoveRole = "users.remove_role";
        public const string Lock = "users.lock";
        public const string Unlock = "users.unlock";
    }

    public static class Roles
    {
        public const string Create = "roles.create";
        public const string Update = "roles.update";
        public const string Delete = "roles.delete";
        public const string View = "roles.view";
        public const string AssignPermission = "roles.assign_permission";
        public const string RemovePermission = "roles.remove_permission";
    }

    public static class PermissionsManagement
    {
        public const string View = "permissions.view";
        public const string Assign = "permissions.assign";
    }

    public static class Auth
    {
        public const string Login = "auth.login";
        public const string Refresh = "auth.refresh";
        public const string Logout = "auth.logout";
    }

    public static class Documents
    {
        public const string Upload = "documents.upload";
        public const string View = "documents.view";
        public const string Delete = "documents.delete";
    }

    public static class Identity
    {
        public const string Manage = "identity.manage";
        public const string View = "identity.view";
    }

    public static class Tenants
    {
        public const string Create = "tenants.create";
        public const string Update = "tenants.update";
        public const string Delete = "tenants.delete";
        public const string View = "tenants.view";
        public const string Approve = "tenants.approve";
    }

    public static class System
    {
        public const string Settings = "system.settings";
        public const string AuditLogs = "system.audit_logs";
    }

    public static class Store
    {
        public const string View = "store.view";
        public const string ViewAll = "store.view_all";
        public const string Update = "store.update";
    }

    public static class Platform
    {
        public const string ViewAllTenants = "platform.tenants.view";
        public const string ManageTenants = "platform.tenants.manage";
        public const string ViewAllStores = "platform.stores.view";
        public const string ManageAnyStore = "platform.stores.manage";
        public const string ViewAllUsers = "platform.users.view";
        public const string ManageAnyUser = "platform.users.manage";
        public const string ViewAuditLogs = "platform.audit.view";
        public const string ManageSystemSettings = "platform.settings.manage";
        public const string CreateImpersonation = "platform.impersonation.create";
        public const string ManageBilling = "platform.billing.manage";
    }

    // Add inside Permissions class, after System
    public static class Catalog
    {
        public const string ProductsView = "catalog.products.view";
        public const string ProductsCreate = "catalog.products.create";
        public const string ProductsUpdate = "catalog.products.update";
        public const string ProductsDelete = "catalog.products.delete";
        public const string CategoriesView = "catalog.categories.view";
        public const string CategoriesCreate = "catalog.categories.create";
        public const string CategoriesUpdate = "catalog.categories.update";
        public const string CategoriesDelete = "catalog.categories.delete";
        public const string TemplatesManagePlatform = "catalog.templates.manage.platform";
        public const string TemplatesManageTenant = "catalog.templates.manage.tenant";
        public const string TemplatesAssign = "catalog.templates.assign";
        public const string TemplatesToggleStore = "catalog.templates.toggle.store";
        public const string TemplatesView = "catalog.templates.view";
    }

    // Update the All list
    public static IReadOnlyList<string> All { get; } =
        typeof(Permissions)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(t => t.GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy))
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .Distinct()
            .OrderBy(x => x)
            .ToList()
            .AsReadOnly();
}