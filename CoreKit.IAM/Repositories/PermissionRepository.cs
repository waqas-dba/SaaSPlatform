// CoreKit.IAM | CoreKit.IAM/Repositories/PermissionRepository.cs
using Microsoft.EntityFrameworkCore;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;

namespace CoreKit.IAM.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly IamDbContext _db;

    public PermissionRepository(IamDbContext db) => _db = db;

    public async Task<bool> HasPermissionAsync(
        Guid userId, Guid? tenantId, string permission)
    {
        return await (
            from ur in _db.UserRoles
            join rp in _db.RolePermissions on ur.RoleId equals rp.RoleId
            join p in _db.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId
               // FIX: global roles (TenantId=null) grant permission everywhere;
               // tenant roles grant permission only in their tenant.
               && (ur.TenantId == null || ur.TenantId == tenantId)
               && p.Name == permission
            select p
        ).AnyAsync();
    }

    public async Task<bool> HasPermissionByModuleAsync(
        Guid userId, Guid? tenantId, string module)
    {
        var prefix = module + ".";
        return await (
            from ur in _db.UserRoles
            join rp in _db.RolePermissions on ur.RoleId equals rp.RoleId
            join p in _db.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId
               && (ur.TenantId == null || ur.TenantId == tenantId)
               && p.Name.StartsWith(prefix)
            select p
        ).AnyAsync();
    }

    public async Task<List<string>> GetPermissionsAsync(
        Guid userId, Guid? tenantId)
    {
        return await (
            from ur in _db.UserRoles
            join rp in _db.RolePermissions on ur.RoleId equals rp.RoleId
            join p in _db.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId
               && (ur.TenantId == null || ur.TenantId == tenantId)
            select p.Name
        )
        .Distinct()
        .ToListAsync();
    }
}