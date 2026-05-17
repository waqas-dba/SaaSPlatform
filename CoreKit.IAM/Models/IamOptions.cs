// CoreKit.IAM/Models/IamOptions.cs
using Microsoft.AspNetCore.Http;

namespace CoreKit.IAM.Models;

public class IamOptions
{
    public string LoginIdentifier { get; set; } = "phone";
    public Func<HttpContext, Guid?>? TenantResolver { get; set; }
    public string SuperAdminRoleName { get; set; } = "SuperAdmin";
    public bool SuperAdminBypassPermissions { get; set; } = true;
    public int MinPasswordLength { get; set; } = 8;
    public bool EnableUserDocuments { get; set; } = false;
    public bool EnableUserIdentities { get; set; } = false;
    public bool RequireCnic { get; set; } = true;
    public bool EnableRoleDocumentRequirements { get; set; } = false;
    public bool AllowDefaultAdminSeed { get; set; } = false;

    // Fix #8: Libraries should not auto-migrate without explicit consent.
    // Set to true only in the Seeder host or in environments where you
    // want the library to manage migrations (e.g. integration test setups).
    // Production APIs should run migrations via a dedicated migration step.
    public bool RunMigrationsOnBootstrap { get; set; } = false;
}