using Microsoft.EntityFrameworkCore;
using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;

namespace AuthCoreKit.IAM.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IIamDbContext _db;

    public RoleRepository(IIamDbContext db) => _db = db;

    public async Task<List<Role>> GetRolesByTenantAsync(Guid? tenantId)
        => await _db.Roles
            .Where(r => r.TenantId == tenantId || (tenantId == null && r.TenantId == null))
            .ToListAsync();

    public async Task<Role?> GetByIdAsync(Guid roleId)
        => await _db.Roles.Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

    public void Add(Role role) => _db.Roles.Add(role);
    public void Update(Role role) => _db.Roles.Update(role);
    public void Delete(Role role) => _db.Roles.Remove(role);

    public async Task<List<Permission>> GetAllPermissionsAsync(CancellationToken ct = default)
        => await _db.Permissions.ToListAsync(ct);

    public void AddRolePermission(RolePermission rp) => _db.RolePermissions.Add(rp);
    public void RemoveRolePermission(RolePermission rp) => _db.RolePermissions.Remove(rp);
    public void AddUserRole(UserRole ur) => _db.UserRoles.Add(ur);
}