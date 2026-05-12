namespace AuthCoreKit.IAM.Interfaces;

public interface IPermissionRepository
{
    Task<bool> HasPermissionAsync(Guid userId, Guid? tenantId, string permission);
    Task<bool> HasPermissionByModuleAsync(Guid userId, Guid? tenantId, string module);
    Task<List<string>> GetPermissionsAsync(Guid userId, Guid? tenantId);
}