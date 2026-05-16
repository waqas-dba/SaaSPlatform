// CoreKit.IAM | CoreKit.IAM/Services/UserManagementService.cs
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IamDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IamOptions _options;

    public UserManagementService(
        IamDbContext db,
        ICurrentUserService currentUser,
        IPasswordHasher passwordHasher,
        IamOptions options)
    {
        _db = db;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
        _options = options;
    }

    public async Task<List<User>> GetUsersAsync(Guid? tenantId)
        => await _db.Users.Where(u => u.TenantId == tenantId).ToListAsync();

    public async Task<User?> GetUserByIdAsync(Guid userId, Guid? tenantId)
        => await _db.Users
            .Include(u => u.Roles).ThenInclude(r => r.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

    public async Task<User> CreateUserAsync(
        string name, string phone, string? email,
        string password, Guid? tenantId)
    {
        if (await _db.Users.AnyAsync(u => u.Phone == phone && u.TenantId == tenantId))
            throw new InvalidOperationException(
                "A user with this phone already exists in the tenant.");

        if (!string.IsNullOrWhiteSpace(email) &&
            await _db.Users.AnyAsync(u => u.Email == email && u.TenantId == tenantId))
            throw new InvalidOperationException(
                "A user with this email already exists in the tenant.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Phone = phone,
            Email = email,
            PasswordHash = _passwordHasher.Hash(password),
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task UpdateUserAsync(
        Guid userId, string? name, string? email,
        string? phone, Guid? tenantId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId)
            ?? throw new KeyNotFoundException("User not found.");

        if (name != null) user.Name = name;
        if (phone != null) user.Phone = phone;
        if (email != null) user.Email = email;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid userId, Guid? tenantId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId)
            ?? throw new KeyNotFoundException("User not found.");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    public async Task AssignRoleAsync(Guid userId, Guid roleId, Guid? tenantId)
    {
        var user = await _db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId)
                   ?? throw new KeyNotFoundException("Role not found.");

        if (role.Name == _options.SuperAdminRoleName && !_currentUser.IsSuperAdmin)
            throw new UnauthorizedAccessException(
                "Only a SuperAdmin can assign this role.");

        if (role.IsSystem && !_currentUser.IsSuperAdmin)
            throw new UnauthorizedAccessException("Cannot assign a system role.");

        // FIX: tenantId=null is now a legitimate value meaning "global scope".
        // No fake GUID fallback needed. SuperAdmin is assigned with null tenantId.
        if (user.Roles.Any(r =>
                r.RoleId == roleId &&
                r.TenantId == tenantId))
            throw new InvalidOperationException(
                "User already has this role in the specified scope.");

        user.Roles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            TenantId = tenantId   // null = global; value = tenant-scoped
        });

        await _db.SaveChangesAsync();
    }

    public async Task RemoveRoleAsync(Guid userId, Guid roleId, Guid? tenantId)
    {
        var user = await _db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        // FIX: match on RoleId AND TenantId (including null == null) so we
        // never remove the wrong tenant's assignment.
        var userRole = user.Roles.FirstOrDefault(r =>
            r.RoleId == roleId &&
            r.TenantId == tenantId);

        if (userRole == null)
            throw new KeyNotFoundException(
                "User does not have this role in the specified scope.");

        user.Roles.Remove(userRole);
        await _db.SaveChangesAsync();
    }

    public async Task LockUserAsync(Guid userId, DateTime lockoutEnd)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                   ?? throw new KeyNotFoundException("User not found.");
        user.LockoutEnd = lockoutEnd;
        await _db.SaveChangesAsync();
    }

    public async Task UnlockUserAsync(Guid userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                   ?? throw new KeyNotFoundException("User not found.");
        user.LockoutEnd = null;
        user.FailedLoginAttempts = 0;
        await _db.SaveChangesAsync();
    }
}