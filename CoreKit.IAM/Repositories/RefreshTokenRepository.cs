// CoreKit.IAM/Repositories/RefreshTokenRepository.cs
using Microsoft.EntityFrameworkCore;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;

namespace CoreKit.IAM.Repositories;

// Fix #2: Repository methods no longer call SaveChangesAsync.
// Callers (RefreshTokenService) are responsible for committing via the DbContext.
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IamDbContext _db;

    public RefreshTokenRepository(IamDbContext db) => _db = db;

    public async Task AddAsync(RefreshToken token)
    {
        await _db.RefreshTokens.AddAsync(token);
        // Removed: _db.SaveChangesAsync() — caller commits.
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        => await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

    public async Task<List<RefreshToken>> GetByFamilyIdAsync(string familyId)
        => await _db.RefreshTokens.Where(x => x.FamilyId == familyId).ToListAsync();

    public void Update(RefreshToken token)
        => _db.RefreshTokens.Update(token);

    public void UpdateRange(IEnumerable<RefreshToken> tokens)
        => _db.RefreshTokens.UpdateRange(tokens);

    // RevokeFamily no longer saves either — just marks state.
    public async Task RevokeFamilyAsync(string familyId, string? revokedByIp = null)
    {
        var tokens = await GetByFamilyIdAsync(familyId);
        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedAtUtc = DateTime.UtcNow;
            t.RevokedByIp = revokedByIp;
        }
        UpdateRange(tokens);
    }
}