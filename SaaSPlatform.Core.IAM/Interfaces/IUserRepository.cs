using SaaSPlatform.Core.IAM.Entities;

namespace SaaSPlatform.Core.IAM.Interfaces;

public interface IUserRepository
{
    // =========================
    // READ
    // =========================

    Task<User?> GetByIdAsync(Guid userId);

    Task<User?> GetByPhoneAsync(string phone, Guid tenantId);

    Task<User?> GetByEmailAsync(string email, Guid tenantId);

    Task<List<User>> GetByTenantAsync(Guid tenantId);

    // =========================
    // EXISTS
    // =========================

    Task<bool> ExistsByPhoneAsync(string phone, Guid tenantId);

    Task<bool> ExistsByEmailAsync(string email, Guid tenantId);

    // =========================
    // WRITE
    // =========================

    Task AddAsync(User user);

    Task UpdateAsync(User user);

    Task DeleteAsync(User user);

    // =========================
    // SECURITY
    // =========================

    Task IncrementFailedLoginAttemptsAsync(Guid userId);

    Task ResetFailedLoginAttemptsAsync(Guid userId);

    Task LockUserAsync(Guid userId, DateTime lockoutEnd);

    Task<DateTime?> GetLockoutEndAsync(Guid userId);
}