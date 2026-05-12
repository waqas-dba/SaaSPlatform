using Microsoft.EntityFrameworkCore;
using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;

namespace AuthCoreKit.IAM.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IIamDbContext _db;

    public RefreshTokenRepository(IIamDbContext db) => _db = db;

    public async Task AddAsync(RefreshToken token)
    {
        await _db.RefreshTokens.AddAsync(token);
        await _db.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
        => await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);

    public async Task UpdateAsync(RefreshToken token)
    {
        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAsync(RefreshToken token)
    {
        token.IsRevoked = true;
        token.RevokedAtUtc = DateTime.UtcNow;
        await UpdateAsync(token);
    }
}