using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.Core.IAM.Services;

namespace SaaSPlatform.Infrastructure.Services.IAM;

public class UserRepository : IUserRepository
{
    private readonly SaaSPlatformDbContext _db;

    public UserRepository(SaaSPlatformDbContext db)
    {
        _db = db;
    }

    // =========================
    // READ
    // =========================

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _db.Users
            .Include(x => x.Roles)
                .ThenInclude(x => x.Role)              // ✅ loads Role
            .FirstOrDefaultAsync(x => x.Id == userId);
    }

    public async Task<User?> GetByPhoneAsync(string phone, Guid tenantId)
    {
        var normalized = PhoneNormalizer.Normalize(phone);

        return await _db.Users
            .Include(x => x.Roles)
                .ThenInclude(x => x.Role)              // ✅ loads Role
            .FirstOrDefaultAsync(x =>
                x.Phone == normalized &&
                x.TenantId == tenantId);
    }

    public async Task<User?> GetByEmailAsync(string email, Guid tenantId)
    {
        return await _db.Users
            .Include(x => x.Roles)
                .ThenInclude(x => x.Role)              // ✅ loads Role
            .FirstOrDefaultAsync(x =>
                x.Email == email &&
                x.TenantId == tenantId);
    }

    public async Task<List<User>> GetByTenantAsync(Guid tenantId)
    {
        return await _db.Users
            .Where(x => x.TenantId == tenantId)
            .ToListAsync();
    }

    // =========================
    // EXISTS (tenant‑scoped)
    // =========================

    public async Task<bool> ExistsByPhoneAsync(string phone, Guid tenantId)
    {
        var normalized = PhoneNormalizer.Normalize(phone);

        return await _db.Users
            .AnyAsync(x =>
                x.Phone == normalized &&
                x.TenantId == tenantId);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid tenantId)
    {
        return await _db.Users
            .AnyAsync(x =>
                x.Email == email &&
                x.TenantId == tenantId);
    }

    // =========================
    // GLOBAL EXISTS (ignore tenant filter)
    // =========================

    public async Task<bool> AnyUserWithPhoneAsync(string phone)
    {
        var normalized = PhoneNormalizer.Normalize(phone);

        return await _db.Users
            .IgnoreQueryFilters()                       // ✅ cross‑tenant check
            .AnyAsync(x => x.Phone == normalized);
    }

    public async Task<bool> AnyUserWithEmailAsync(string email)
    {
        return await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Email == email);
    }

    // =========================
    // WRITE
    // =========================

    // Now synchronous – only tracks the entity, no immediate save
    public void Add(User user)
    {
        _db.Users.Add(user);
    }

    public async Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    // =========================
    // SECURITY
    // =========================

    public async Task IncrementFailedLoginAttemptsAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return;

        user.FailedLoginAttempts++;
        await _db.SaveChangesAsync();
    }

    public async Task ResetFailedLoginAttemptsAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return;

        user.FailedLoginAttempts = 0;
        await _db.SaveChangesAsync();
    }

    public async Task LockUserAsync(Guid userId, DateTime lockoutEnd)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return;

        user.LockoutEnd = lockoutEnd;
        await _db.SaveChangesAsync();
    }

    public async Task<DateTime?> GetLockoutEndAsync(Guid userId)
    {
        return await _db.Users
            .Where(x => x.Id == userId)
            .Select(x => x.LockoutEnd)
            .FirstOrDefaultAsync();
    }
}