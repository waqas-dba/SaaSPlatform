using Microsoft.EntityFrameworkCore;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;

namespace CoreKit.IAM.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly IamDbContext _db;

    public PermissionRepository(IamDbContext db) => _db = db;

    public async Task<bool> HasPermissionAsync(Guid userId, Guid? tenantId, string permission)
        => await _db.UserRoles
            .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
            .SelectMany(ur => ur.Role!.RolePermissions!)
            .AnyAsync(rp => rp.Permission != null && rp.Permission.Name == permission);

    public async Task<bool> HasPermissionByModuleAsync(Guid userId, Guid? tenantId, string module)
    {
        var prefix = module + ".";
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
            .SelectMany(ur => ur.Role!.RolePermissions!)
            .AnyAsync(rp => rp.Permission != null && rp.Permission.Name.StartsWith(prefix));
    }

    public async Task<List<string>> GetPermissionsAsync(Guid userId, Guid? tenantId)
        => await _db.UserRoles
            .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
            .SelectMany(ur => ur.Role!.RolePermissions!)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Name)
            .Distinct()
            .ToListAsync();
}