using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.IAM.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly SaaSPlatformDbContext _db;
    public RoleRepository(SaaSPlatformDbContext db) => _db = db;

    public void Add(Role role) => _db.Roles.Add(role);
    public async Task<List<Permission>> GetAllPermissionsAsync(CancellationToken ct)
        => await _db.Permissions.ToListAsync(ct);
    public void AddRolePermission(RolePermission rp) => _db.RolePermissions.Add(rp);
    public void AddUserRole(UserRole ur) => _db.UserRoles.Add(ur);
    public void AddTenantUser(TenantUser tu) => _db.TenantUsers.Add(tu);
}