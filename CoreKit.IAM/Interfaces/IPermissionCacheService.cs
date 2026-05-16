// CoreKit.IAM | CoreKit.IAM/Interfaces/IPermissionCacheService.cs
namespace CoreKit.IAM.Interfaces;

public interface IPermissionCacheService
{
    Task<List<string>?> GetAsync(Guid userId, Guid? tenantId);
    Task SetAsync(Guid userId, Guid? tenantId, List<string> permissions);
    Task InvalidateAsync(Guid userId, Guid? tenantId);
    Task InvalidateUserAsync(Guid userId);
}