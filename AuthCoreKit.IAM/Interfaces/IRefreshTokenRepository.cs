using AuthCoreKit.IAM.Entities;

namespace AuthCoreKit.IAM.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task UpdateAsync(RefreshToken token);
        Task RevokeAsync(RefreshToken token);
    }
}