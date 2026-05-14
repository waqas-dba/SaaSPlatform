using CoreKit.IAM.Interfaces;

namespace CoreKit.IAM.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repo;

    public PermissionService(IPermissionRepository repo) => _repo = repo;

    public Task<bool> HasPermissionAsync(Guid userId, Guid? tenantId, string permission)
        => _repo.HasPermissionAsync(userId, tenantId, permission);

    public Task<bool> HasModuleAccessAsync(Guid userId, Guid? tenantId, string module)
        => _repo.HasPermissionByModuleAsync(userId, tenantId, module);

    public Task<List<string>> GetUserPermissionsAsync(Guid userId, Guid? tenantId)
        => _repo.GetPermissionsAsync(userId, tenantId);
}