// CoreKit.IAM/Interfaces/IRefreshTokenRepository.cs
using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<List<RefreshToken>> GetByFamilyIdAsync(string familyId);
    void Update(RefreshToken token);           // was: Task UpdateAsync
    void UpdateRange(IEnumerable<RefreshToken> tokens); // was: Task UpdateRangeAsync
    Task RevokeFamilyAsync(string familyId, string? revokedByIp = null);
}