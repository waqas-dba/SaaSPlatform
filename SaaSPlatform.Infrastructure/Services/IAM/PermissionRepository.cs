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
            .IgnoreQueryFilters()
            .Where(x =>
                x.UserId == userId &&
                x.TenantId == tenantId)
            .SelectMany(x => x.Role!.RolePermissions!)
            .AnyAsync(x =>
                x.Permission != null &&
                x.Permission.Name == permission);
    }

    public async Task<bool> HasPermissionByModuleAsync(
        Guid userId,
        Guid tenantId,
        string module)
    {
        var prefix = module + ".";

        return await _db.UserRoles
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(x =>
                x.UserId == userId &&
                x.TenantId == tenantId)
            .SelectMany(x => x.Role!.RolePermissions!)
            .AnyAsync(x =>
                x.Permission != null &&
                x.Permission.Name.StartsWith(prefix));
    }

    public async Task<List<string>> GetPermissionsAsync(
        Guid userId,
        Guid tenantId)
    {
        return await _db.UserRoles
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(x =>
                x.UserId == userId &&
                x.TenantId == tenantId)
            .SelectMany(x => x.Role!.RolePermissions!)
            .Where(x => x.Permission != null)
            .Select(x => x.Permission!.Name)
            .Distinct()
            .ToListAsync();
    }
}