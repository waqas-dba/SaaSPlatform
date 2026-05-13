using AuthCoreKit.IAM.Entities;

namespace AuthCoreKit.IAM.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);

    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

    Task<List<RefreshToken>> GetByFamilyIdAsync(string familyId);

    Task UpdateAsync(RefreshToken token);

    Task UpdateRangeAsync(IEnumerable<RefreshToken> tokens);

    Task RevokeFamilyAsync(string familyId, string? revokedByIp = null);
}