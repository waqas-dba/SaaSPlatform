using Microsoft.EntityFrameworkCore;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Helpers;
using CoreKit.IAM.Persistence;

namespace CoreKit.IAM.Repositories;

/// <summary>
/// Manages refresh token persistence.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IamDbContext _db;

    public RefreshTokenRepository(IamDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(RefreshToken token)
    {
        await _db.RefreshTokens.AddAsync(token);
        await _db.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        => await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

    public async Task<List<RefreshToken>> GetByFamilyIdAsync(string familyId)
        => await _db.RefreshTokens.Where(x => x.FamilyId == familyId).ToListAsync();

    public async Task UpdateAsync(RefreshToken token)
    {
        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<RefreshToken> tokens)
    {
        _db.RefreshTokens.UpdateRange(tokens);
        await _db.SaveChangesAsync();
    }

    public async Task RevokeFamilyAsync(string familyId, string? revokedByIp = null)
    {
        var tokens = await GetByFamilyIdAsync(familyId);
        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedAtUtc = DateTime.UtcNow;
            t.RevokedByIp = revokedByIp;
        }
        await UpdateRangeAsync(tokens);
    }
}