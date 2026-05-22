namespace CoreKit.IAM.Models;

public class IamOptions
{
    public string LoginIdentifier { get; set; } = "phone";
    public Func<Microsoft.AspNetCore.Http.HttpContext, Guid?>? TenantResolver { get; set; }
    public int MinPasswordLength { get; set; } = 8;
    public bool EnableUserDocuments { get; set; } = false;
    public bool EnableUserIdentities { get; set; } = false;
    public bool RequireCnic { get; set; } = true;
    public bool EnableRoleDocumentRequirements { get; set; } = false;
    public bool AllowDefaultAdminSeed { get; set; } = false;
    public bool RunMigrationsOnBootstrap { get; set; } = false;
    public string PlatformAdminRoleName { get; set; } = "PlatformAdmin";
    public bool EnableImpersonation { get; set; } = true;
    public bool RequireImpersonationForCrossTenant { get; set; } = true;
}