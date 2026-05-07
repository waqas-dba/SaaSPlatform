
namespace SaaSPlatform.Core.IAM.Interfaces;

public interface IPermissionRepository
{
    Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission);

    Task<bool> HasPermissionByModuleAsync(Guid userId, Guid tenantId, string module);
}