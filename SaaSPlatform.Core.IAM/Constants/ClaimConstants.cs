using System.Security.Claims;

namespace SaaSPlatform.Core.IAM.Constants;

public static class ClaimConstants
{
    public const string UserId = "userId";

    public const string TenantId = "tenantId";

    public const string Permission = "permission";

    public const string Email = ClaimTypes.Email;

    public const string Role = ClaimTypes.Role;

    public const string Name = ClaimTypes.Name;

    public const string NameIdentifier = ClaimTypes.NameIdentifier;

    public const string Subject = "sub";

    public const string JwtId = "jti";
}