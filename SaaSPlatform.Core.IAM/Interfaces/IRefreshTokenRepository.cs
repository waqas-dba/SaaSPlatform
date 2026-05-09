using SaaSPlatform.Core.IAM.Entities;

namespace SaaSPlatform.Core.IAM.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);

    Task<RefreshToken?> GetByTokenAsync(string token);

    Task<List<RefreshToken>> GetUserTokensAsync(Guid userId);

    Task RevokeAsync(RefreshToken token);

    Task SaveChangesAsync();
}