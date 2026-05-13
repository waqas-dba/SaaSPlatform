namespace AuthCoreKit.IAM.Interfaces;

public interface IUserIdentityService
{
    Task SetCnicAsync(Guid userId, string plainCnic);
    Task<string?> GetCnicAsync(Guid userId);
    Task DeleteAsync(Guid userId);
}