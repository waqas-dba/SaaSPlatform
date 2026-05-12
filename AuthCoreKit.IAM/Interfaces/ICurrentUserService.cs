namespace AuthCoreKit.IAM.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    bool IsAuthenticated { get; }
    bool IsSuperAdmin { get; }
    bool HasPermission(string permission);
}