using Microsoft.EntityFrameworkCore;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Helpers;
using CoreKit.IAM.Persistence;

namespace CoreKit.IAM.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IamDbContext _db;

    public UserRepository(IamDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _db.Users
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByPhoneAsync(
        string phone,
        Guid? tenantId = null)
    {
        phone = PhoneNormalizer.Normalize(phone);

        return await _db.Users
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u =>
                u.Phone == phone &&
                u.TenantId == tenantId);
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        Guid? tenantId = null)
    {
        email = email.Trim().ToLowerInvariant();

        return await _db.Users
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.TenantId == tenantId);
    }

    public async Task<List<User>> GetByTenantAsync(Guid tenantId)
    {
        return await _db.Users
            .Where(u => u.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<bool> ExistsByPhoneAsync(
        string phone,
        Guid? tenantId = null)
    {
        phone = PhoneNormalizer.Normalize(phone);

        return await _db.Users.AnyAsync(u =>
            u.Phone == phone &&
            u.TenantId == tenantId);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        Guid? tenantId = null)
    {
        email = email.Trim().ToLowerInvariant();

        return await _db.Users.AnyAsync(u =>
            u.Email == email &&
            u.TenantId == tenantId);
    }

    public void Add(User user)
    {
        _db.Users.Add(user);
    }

    public void Update(User user)
    {
        _db.Users.Update(user);
    }

    public void Delete(User user)
    {
        _db.Users.Remove(user);
    }

    public async Task<User?> GetUserByLoginAsync(
        string login,
        Guid? tenantId,
        string loginIdentifier)
    {
        switch (loginIdentifier.ToLowerInvariant())
        {
            case "email":
                return await GetByEmailAsync(login, tenantId);

            case "both":
                return login.Contains('@')
                    ? await GetByEmailAsync(login, tenantId)
                    : await GetByPhoneAsync(login, tenantId);

            default:
                return await GetByPhoneAsync(login, tenantId);
        }
    }
}