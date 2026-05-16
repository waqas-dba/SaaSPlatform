// CoreKit.IAM/Repositories/RoleRepository.cs
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IamDbContext _db;

    public RoleRepository(IamDbContext db) => _db = db;

    public async Task<List<Role>> GetRolesByTenantAsync(Guid? tenantId)
        => await _db.Roles
            .Where(r => r.TenantId == tenantId)
            // FIX: Include the Permission navigation so callers get names,
            // not just PermissionId GUIDs.
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync();

    public async Task<Role?> GetByIdAsync(Guid roleId)
        => await _db.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == roleId);

    public void Add(Role role) => _db.Roles.Add(role);
    public void Update(Role role) => _db.Roles.Update(role);
    public void Delete(Role role) => _db.Roles.Remove(role);

    public async Task<List<Permission>> GetAllPermissionsAsync(
        CancellationToken ct = default)
        => await _db.Permissions.ToListAsync(ct);

    public void AddRolePermission(RolePermission rp) => _db.RolePermissions.Add(rp);
    public void RemoveRolePermission(RolePermission rp) => _db.RolePermissions.Remove(rp);
    public void AddUserRole(UserRole ur) => _db.UserRoles.Add(ur);
}