using Microsoft.EntityFrameworkCore;
using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace AuthCoreKit.IAM.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IIamDbContext _db;

    public RefreshTokenRepository(IIamDbContext db)
    {
        _db = db;
    }

    private static string Hash(string token)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
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