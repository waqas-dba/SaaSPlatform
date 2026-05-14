namespace CoreKit.IAM.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, Guid? tenantId, string permission);
    Task<bool> HasModuleAccessAsync(Guid userId, Guid? tenantId, string module);
    Task<List<string>> GetUserPermissionsAsync(Guid userId, Guid? tenantId);
}