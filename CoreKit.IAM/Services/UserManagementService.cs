using CoreKit.IAM.Constants;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Helpers;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
using CoreKit.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IamDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IamOptions _options;
    private readonly IPermissionCacheService _permissionCache;

    public UserManagementService(
        IamDbContext db,
        ICurrentUserService currentUser,
        IPasswordHasher passwordHasher,
        IamOptions options,
        IPermissionCacheService permissionCache)
    {
        _db = db;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
        _options = options;
        _permissionCache = permissionCache;
    }

    public async Task<User> CreateUserAsync(
        string name, string phone, string? email,
        string password, Guid? tenantId)
    {
        phone = PhoneNormalizer.Normalize(phone);
        email = string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Phone == phone && u.TenantId == tenantId))
            throw new InvalidOperationException(
                "A user with this phone already exists in the tenant.");

        if (email != null &&
            await _db.Users.AnyAsync(u => u.Email == email && u.TenantId == tenantId))
            throw new InvalidOperationException(
                "A user with this email already exists in the tenant.");

        if (password.Length < _options.MinPasswordLength)
            throw new ArgumentException(
                $"Password must be at least {_options.MinPasswordLength} characters.");

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

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new InvalidOperationException(
                "A user with this phone or email already exists. Please use different credentials.");
        }

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

        if (phone != null)
        {
            var normalizedPhone = PhoneNormalizer.Normalize(phone);
            var phoneInUse = await _db.Users.AnyAsync(u =>
                u.Phone == normalizedPhone &&
                u.TenantId == tenantId &&
                u.Id != userId);

            if (phoneInUse)
                throw new InvalidOperationException("Phone already in use.");

            user.Phone = normalizedPhone;
        }

        if (email != null)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var emailInUse = await _db.Users.AnyAsync(u =>
                u.Email == normalizedEmail &&
                u.TenantId == tenantId &&
                u.Id != userId);

            if (emailInUse)
                throw new InvalidOperationException("Email already in use.");

            user.Email = normalizedEmail;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<User>> GetUsersAsync(Guid? tenantId)
        => await _db.Users.Where(u => u.TenantId == tenantId).ToListAsync();

    public async Task<User?> GetUserByIdAsync(Guid userId, Guid? tenantId)
        => await _db.Users
            .Include(u => u.Roles).ThenInclude(r => r.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

    public async Task DeleteUserAsync(Guid userId, Guid? tenantId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId)
            ?? throw new KeyNotFoundException("User not found.");

        user.IsDeleted = true;
        user.IsActive = false;

        await _db.SaveChangesAsync();
        await _permissionCache.InvalidateUserAsync(userId);
    }

    public async Task AssignRoleAsync(Guid userId, Guid roleId, Guid? tenantId)
    {
        var user = await _db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId)
            ?? throw new KeyNotFoundException("Role not found.");

        // Only platform admins with manage permission can assign platform roles
        if (role.TenantId == null && !_currentUser.HasPermission(Permissions.Platform.ManageAnyUser))
            throw new ForbiddenException("Only platform administrators can assign platform-level roles.");

        // System roles require platform permission
        if (role.IsSystem && !_currentUser.HasPermission(Permissions.Platform.ManageAnyUser))
            throw new ForbiddenException("Cannot assign a system role.");

        if (user.Roles.Any(r => r.RoleId == roleId && r.TenantId == tenantId))
            throw new InvalidOperationException(
                "User already has this role in the specified scope.");

        user.Roles.Add(new UserRole { UserId = userId, RoleId = roleId, TenantId = tenantId });

        await _db.SaveChangesAsync();
        await _permissionCache.InvalidateUserAsync(userId);
    }

    public async Task RemoveRoleAsync(Guid userId, Guid roleId, Guid? tenantId)
    {
        var user = await _db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        var userRole = user.Roles.FirstOrDefault(r =>
            r.RoleId == roleId && r.TenantId == tenantId)
            ?? throw new KeyNotFoundException(
                "User does not have this role in the specified scope.");

        user.Roles.Remove(userRole);

        await _db.SaveChangesAsync();
        await _permissionCache.InvalidateUserAsync(userId);
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


    public async Task<List<UserListItem>> GetAllUsersAsync()
    {
        return await _db.Users
            .AsNoTracking()
            .Select(u => new UserListItem
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                IsActive = u.IsActive,
                TenantId = u.TenantId
            })
            .ToListAsync();
    }
}