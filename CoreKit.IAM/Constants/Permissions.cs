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
        public const string SuperAdmin = "system.super_admin";
        public const string Settings = "system.settings";
        public const string AuditLogs = "system.audit_logs";
    }

    public static class Store
    {
        public const string View = "store.view";
        public const string Update = "store.update";
    }

    /// <summary>
    /// Returns all permissions as a flat list.
    /// Useful for seeding database permissions.
    /// </summary>
    public static IReadOnlyList<string> All => new[]
    {
        // Users
        Users.Create,
        Users.Update,
        Users.Delete,
        Users.View,
        Users.AssignRole,
        Users.RemoveRole,
        Users.Lock,
        Users.Unlock,

        // Roles
        Roles.Create,
        Roles.Update,
        Roles.Delete,
        Roles.View,
        Roles.AssignPermission,
        Roles.RemovePermission,

        // Permissions
        PermissionsManagement.View,
        PermissionsManagement.Assign,

        // Auth
        Auth.Login,
        Auth.Refresh,
        Auth.Logout,

        // Documents
        Documents.Upload,
        Documents.View,
        Documents.Delete,

        // Identity
        Identity.Manage,
        Identity.View,

        // Tenants
        Tenants.Create,
        Tenants.Update,
        Tenants.Delete,
        Tenants.View,
        Tenants.Approve,

        // System
        System.SuperAdmin,
        System.Settings,
        System.AuditLogs,

        Store.Update,
        Store.View
    };
}