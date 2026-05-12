using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthCoreKit.IAM.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly IIamDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IamOptions _options;

    public RoleManagementService(IIamDbContext db, ICurrentUserService currentUser, IamOptions options)
    {
        _db = db;
        _currentUser = currentUser;
        _options = options;
    }

    public async Task<List<Role>> GetRolesAsync(Guid? tenantId)
        => await _db.Roles.Where(r => r.TenantId == tenantId).ToListAsync();

    public async Task<Role?> GetRoleByIdAsync(Guid roleId, Guid? tenantId)
        => await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId);

    public async Task<Role> CreateRoleAsync(string name, Guid? tenantId, string? description = null)
    {
        if (await _db.Roles.AnyAsync(r => r.Name == name && r.TenantId == tenantId))
            throw new InvalidOperationException("A role with this name already exists in the tenant.");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            IsSystem = false
        };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task UpdateRoleAsync(Guid roleId, string? newName, string? newDescription, Guid? tenantId)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId)
                   ?? throw new KeyNotFoundException("Role not found.");
        if (role.IsSystem)
            throw new UnauthorizedAccessException("System roles cannot be modified.");

        if (newName != null) role.Name = newName;
        if (newDescription != null) role.Description = newDescription;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteRoleAsync(Guid roleId, Guid? tenantId)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId)
                   ?? throw new KeyNotFoundException("Role not found.");
        if (role.IsSystem)
            throw new UnauthorizedAccessException("System roles cannot be deleted.");

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
    }

    public async Task AssignPermissionAsync(Guid roleId, Guid permissionId)
    {
        var role = await _db.Roles.Include(r => r.RolePermissions)
                        .FirstOrDefaultAsync(r => r.Id == roleId)
                   ?? throw new KeyNotFoundException("Role not found.");
        var permission = await _db.Permissions.FindAsync(permissionId)
                         ?? throw new KeyNotFoundException("Permission not found.");

        if (role.RolePermissions.Any(rp => rp.PermissionId == permissionId))
            throw new InvalidOperationException("Permission already assigned.");

        role.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
        await _db.SaveChangesAsync();
    }

    public async Task RemovePermissionAsync(Guid roleId, Guid permissionId)
    {
        var rp = await _db.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId)
            ?? throw new KeyNotFoundException("Role does not have this permission.");
        _db.RolePermissions.Remove(rp);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Permission>> GetPermissionsForRoleAsync(Guid roleId)
        => await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync();
}