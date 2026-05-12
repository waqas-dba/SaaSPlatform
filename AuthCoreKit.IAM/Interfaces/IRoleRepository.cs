using AuthCoreKit.IAM.Entities;

namespace AuthCoreKit.IAM.Interfaces;

public interface IRoleRepository
{
    Task<List<Role>> GetRolesByTenantAsync(Guid? tenantId);
    Task<Role?> GetByIdAsync(Guid roleId);
    void Add(Role role);
    void Update(Role role);
    void Delete(Role role);
    Task<List<Permission>> GetAllPermissionsAsync(CancellationToken ct = default);
    void AddRolePermission(RolePermission rolePermission);
    void RemoveRolePermission(RolePermission rolePermission);
    void AddUserRole(UserRole userRole);
}