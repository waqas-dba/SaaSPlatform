using AuthCoreKit.IAM.Entities;

namespace AuthCoreKit.IAM.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByPhoneAsync(string phone, Guid? tenantId = null);
    Task<User?> GetByEmailAsync(string email, Guid? tenantId = null);
    Task<List<User>> GetByTenantAsync(Guid tenantId);
    Task<bool> ExistsByPhoneAsync(string phone, Guid? tenantId = null);
    Task<bool> ExistsByEmailAsync(string email, Guid? tenantId = null);
    void Add(User user);
    void Update(User user);
    void Delete(User user);

    /// <summary>
    /// Retrieves a user for login purposes, respecting the configured login identifier (phone, email, or both).
    /// If both, the input is checked for '@' to decide which field to query.
    /// </summary>
    Task<User?> GetUserByLoginAsync(string login, Guid? tenantId, string loginIdentifier);
}