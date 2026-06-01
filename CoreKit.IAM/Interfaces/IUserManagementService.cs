using CoreKit.IAM.Entities;
using CoreKit.IAM.Models;

namespace CoreKit.IAM.Interfaces;

public interface IUserManagementService
{
    Task<List<User>> GetUsersAsync(Guid? tenantId);
    Task<User?> GetUserByIdAsync(Guid userId, Guid? tenantId);
    Task<User> CreateUserAsync(string name, string phone, string? email, string password, Guid? tenantId);
    Task UpdateUserAsync(Guid userId, string? name, string? email, string? phone, Guid? tenantId);
    Task DeleteUserAsync(Guid userId, Guid? tenantId);
    Task AssignRoleAsync(Guid userId, Guid roleId, Guid? tenantId);
    Task RemoveRoleAsync(Guid userId, Guid roleId, Guid? tenantId);
    Task LockUserAsync(Guid userId, DateTime lockoutEnd);
    Task UnlockUserAsync(Guid userId);

    Task<List<UserListItem>> GetAllUsersAsync();
}