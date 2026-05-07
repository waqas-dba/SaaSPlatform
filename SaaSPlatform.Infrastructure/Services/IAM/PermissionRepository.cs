using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Infrastructure.Persistence;

namespace SaaSPlatform.Infrastructure.Services.IAM;

public class PermissionRepository : IPermissionRepository
{
    private readonly SaaSPlatformDbContext _db;

    public PermissionRepository(SaaSPlatformDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        Guid tenantId,
        string permission)
    {
        return await _db.UserRoles
            .AsNoTracking()
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions!)
                    .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
            .SelectMany(ur => ur.Role!.RolePermissions!)
            .AnyAsync(rp => rp.Permission!.Name == permission);
    }

    public async Task<bool> HasPermissionByModuleAsync(
        Guid userId,
        Guid tenantId,
        string module)
    {
        return await _db.UserRoles
            .AsNoTracking()
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions!)
                    .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
            .SelectMany(ur => ur.Role!.RolePermissions!)
            .AnyAsync(rp => rp.Permission!.Name.StartsWith(module + "."));
    }
}