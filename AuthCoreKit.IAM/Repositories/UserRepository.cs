using Microsoft.EntityFrameworkCore;
using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Helpers;

namespace AuthCoreKit.IAM.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IIamDbContext _db;

    public UserRepository(IIamDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(Guid userId)
        => await _db.Users.Include(u => u.Roles).ThenInclude(r => r.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

    public async Task<User?> GetByPhoneAsync(string phone, Guid? tenantId = null)
    {
        var normalized = PhoneNormalizer.Normalize(phone);
        var query = _db.Users.Include(u => u.Roles).ThenInclude(r => r.Role)
                        .Where(u => u.Phone == normalized);
        query = tenantId.HasValue
            ? query.Where(u => u.TenantId == tenantId.Value)
            : query.Where(u => u.TenantId == null);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email, Guid? tenantId = null)
    {
        var query = _db.Users.Include(u => u.Roles).ThenInclude(r => r.Role)
                        .Where(u => u.Email == email);
        query = tenantId.HasValue
            ? query.Where(u => u.TenantId == tenantId.Value)
            : query.Where(u => u.TenantId == null);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<User>> GetByTenantAsync(Guid tenantId)
        => await _db.Users.Where(u => u.TenantId == tenantId).ToListAsync();

    public async Task<bool> ExistsByPhoneAsync(string phone, Guid? tenantId = null)
    {
        var normalized = PhoneNormalizer.Normalize(phone);
        var query = _db.Users.Where(u => u.Phone == normalized);
        query = tenantId.HasValue
            ? query.Where(u => u.TenantId == tenantId.Value)
            : query.Where(u => u.TenantId == null);
        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? tenantId = null)
    {
        var query = _db.Users.Where(u => u.Email == email);
        query = tenantId.HasValue
            ? query.Where(u => u.TenantId == tenantId.Value)
            : query.Where(u => u.TenantId == null);
        return await query.AnyAsync();
    }

    public void Add(User user) => _db.Users.Add(user);
    public void Update(User user) => _db.Users.Update(user);
    public void Delete(User user) => _db.Users.Remove(user);

    public async Task<User?> GetUserByLoginAsync(string login, Guid? tenantId, string loginIdentifier)
    {
        switch (loginIdentifier.ToLowerInvariant())
        {
            case "email":
                return await GetByEmailAsync(login, tenantId);
            case "both":
                if (login.Contains('@'))
                    return await GetByEmailAsync(login, tenantId);
                return await GetByPhoneAsync(login, tenantId);
            default: // "phone"
                return await GetByPhoneAsync(login, tenantId);
        }
    }
}