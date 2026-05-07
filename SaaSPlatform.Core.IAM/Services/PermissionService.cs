using SaaSPlatform.Core.IAM.Interfaces;

namespace SaaSPlatform.Core.IAM.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repo;

    public PermissionService(IPermissionRepository repo)
    {
        _repo = repo;
    }

    public Task<bool> HasPermissionAsync(
        Guid userId,
        Guid tenantId,
        string permission)
    {
        return _repo.HasPermissionAsync(userId, tenantId, permission);

    }

    public async Task<bool> HasModuleAccessAsync(
    Guid userId,
    Guid tenantId,
    string module)
    {
        return await _repo.HasPermissionByModuleAsync(userId, tenantId, module);
    }
}