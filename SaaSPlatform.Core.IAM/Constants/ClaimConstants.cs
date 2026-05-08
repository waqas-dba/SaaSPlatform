namespace SaaSPlatform.Core.IAM.Constants;

/// <summary>
/// JWT claim names.
/// Prevents magic strings.
/// </summary>
public static class ClaimConstants
{
    public const string UserId = "userId";

    public const string TenantId = "tenantId";

    public const string Email = "email";

    public const string Role = "role";

    public const string Permission = "permission";
}