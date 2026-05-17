using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly IamDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionCacheService _permissionCache;

    public RoleManagementService(
        IamDbContext db,
        ICurrentUserService currentUser,
        IPermissionCacheService permissionCache)
    {
        _db = db;
        _currentUser = currentUser;
        _permissionCache = permissionCache;
    }

    public Task<List<Role>> GetRolesAsync(Guid? tenantId)
        => _db.Roles.Where(r => r.TenantId == tenantId).ToListAsync();

    public Task<Role?> GetRoleByIdAsync(Guid roleId, Guid? tenantId)
        => _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId);

    public async Task<Role> CreateRoleAsync(
        string name, Guid? tenantId, string? description = null)
    {
        if (await _db.Roles.AnyAsync(r => r.Name == name && r.TenantId == tenantId))
            throw new InvalidOperationException("Role already exists.");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            TenantId = tenantId
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task UpdateRoleAsync(
        Guid roleId, string? newName, string? newDescription, Guid? tenantId)
    {
        var role = await _db.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId)
            ?? throw new KeyNotFoundException("Role not found.");

        GuardSystemRole(role, "modified");

        if (!string.IsNullOrWhiteSpace(newName)) role.Name = newName;
        if (newDescription != null) role.Description = newDescription;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteRoleAsync(Guid roleId, Guid? tenantId)
    {
        var role = await _db.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId)
            ?? throw new KeyNotFoundException("Role not found.");

        GuardSystemRole(role, "deleted");

        // Invalidate cache for every user who had this role
        var affectedUserIds = role.UserRoles.Select(ur => ur.UserId).Distinct().ToList();

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();

        // Invalidate after save so stale data is never served
        foreach (var userId in affectedUserIds)
            await _permissionCache.InvalidateUserAsync(userId);
    }

    public async Task AssignPermissionAsync(Guid roleId, Guid permissionId)
    {
        var role = await _db.Roles
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == roleId)
            ?? throw new KeyNotFoundException("Role not found.");

        GuardSystemRole(role, "modified");

        if (role.RolePermissions.Any(p => p.PermissionId == permissionId))
            return;

        role.RolePermissions.Add(new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        });

        await _db.SaveChangesAsync();

        // Invalidate cache for every user who has this role
        var affectedUserIds = role.UserRoles.Select(ur => ur.UserId).Distinct().ToList();
        foreach (var userId in affectedUserIds)
            await _permissionCache.InvalidateUserAsync(userId);
    }

    public async Task RemovePermissionAsync(Guid roleId, Guid permissionId)
    {
        var role = await _db.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == roleId)
            ?? throw new KeyNotFoundException("Role not found.");

        GuardSystemRole(role, "modified");

        var rp = await _db.RolePermissions
            .FirstOrDefaultAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);

        if (rp == null) return;

        _db.RolePermissions.Remove(rp);
        await _db.SaveChangesAsync();

        // Invalidate cache for every user who has this role
        var affectedUserIds = role.UserRoles.Select(ur => ur.UserId).Distinct().ToList();
        foreach (var userId in affectedUserIds)
            await _permissionCache.InvalidateUserAsync(userId);
    }

    public async Task<List<Permission>> GetPermissionsForRoleAsync(Guid roleId)
        => await _db.RolePermissions
            .Where(x => x.RoleId == roleId)
            .Select(x => x.Permission)
            .ToListAsync();

    private static void GuardSystemRole(Role role, string action)
    {
        if (role.IsSystem)
            throw new InvalidOperationException(
                $"System role '{role.Name}' cannot be {action}.");
    }
}