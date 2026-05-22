using CoreKit.SharedKernel.Common;

namespace CoreKit.IAM.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsPlatformAdmin { get; }
    bool HasPermission(string permission);
    TenantScope GetTenantScope();
    Task<StoreScope> GetStoreScopeAsync();
}