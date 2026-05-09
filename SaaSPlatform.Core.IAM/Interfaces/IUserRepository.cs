using SaaSPlatform.Core.IAM.Entities;

namespace SaaSPlatform.Core.IAM.Interfaces;

public interface IUserRepository
{
    // =========================
    // READ OPERATIONS
    // =========================

    Task<User?> GetByIdAsync(Guid userId);

    Task<User?> GetByPhoneAsync(string phone, Guid tenantId);

    Task<User?> GetByEmailAsync(string email, Guid tenantId);

    Task<List<User>> GetByTenantAsync(Guid tenantId);

    // =========================
    // EXISTENCE CHECKS
    // =========================

    Task<bool> ExistsByPhoneAsync(string phone, Guid tenantId);

    Task<bool> ExistsByEmailAsync(string email, Guid tenantId);

    // =========================
    // WRITE OPERATIONS
    // =========================

    Task AddAsync(User user);

    Task UpdateAsync(User user);

    Task DeleteAsync(User user);

    // =========================
    // SECURITY (AUTH SYSTEM)
    // =========================

    Task IncrementFailedLoginAttemptsAsync(Guid userId);

    Task ResetFailedLoginAttemptsAsync(Guid userId);

    Task LockUserAsync(Guid userId, DateTime lockoutEnd);

    Task<DateTime?> GetLockoutEndAsync(Guid userId);
}