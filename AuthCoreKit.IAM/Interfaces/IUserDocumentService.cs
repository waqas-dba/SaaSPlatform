using AuthCoreKit.IAM.Entities;

namespace AuthCoreKit.IAM.Interfaces;

public interface IUserDocumentService
{
    Task AddOrUpdateAsync(Guid userId, string documentType, string imageUrl);
    Task<UserDocument?> GetAsync(Guid userId, string documentType);
    Task<List<UserDocument>> GetByUserAsync(Guid userId);
    Task DeleteAsync(Guid userId, string documentType);
}