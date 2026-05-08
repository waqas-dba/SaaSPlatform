using SaaSPlatform.Core.IAM.Entities;

namespace SaaSPlatform.Core.IAM.Interfaces;

public interface IRoleRepository
{
    void Add(Role role);
    Task<List<Permission>> GetAllPermissionsAsync(CancellationToken ct = default);
    void AddRolePermission(RolePermission rolePermission);
    void AddUserRole(UserRole userRole);
    void AddTenantUser(TenantUser tenantUser);
}