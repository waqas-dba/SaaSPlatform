using SaaSPlatform.Core.IAM.Entities;

namespace SaaSPlatform.Core.IAM.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByPhoneAsync(string phone, Guid tenantId);
    Task<User?> GetByEmailAsync(string email, Guid tenantId);
    Task<List<User>> GetByTenantAsync(Guid tenantId);
    Task<bool> ExistsByPhoneAsync(string phone, Guid tenantId);
    Task<bool> ExistsByEmailAsync(string email, Guid tenantId);
    void Add(User user);                          // changed from AddAsync to void for consistency
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task IncrementFailedLoginAttemptsAsync(Guid userId);
    Task ResetFailedLoginAttemptsAsync(Guid userId);
    Task LockUserAsync(Guid userId, DateTime lockoutEnd);
    Task<DateTime?> GetLockoutEndAsync(Guid userId);

    // NEW – global checks (ignore tenant filter)
    Task<bool> AnyUserWithPhoneAsync(string phone);
    Task<bool> AnyUserWithEmailAsync(string email);
}