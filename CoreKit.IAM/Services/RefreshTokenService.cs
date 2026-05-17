using System.Security.Cryptography;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Helpers;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoreKit.IAM.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repo;
    private readonly IamDbContext _db;
    private readonly JwtSettings _settings;

    public RefreshTokenService(
        IRefreshTokenRepository repo,
        IamDbContext db,
        IOptions<JwtSettings> options)
    {
        _repo = repo;
        _db = db;
        _settings = options.Value;
    }

    public async Task<string> GenerateAsync(
        Guid userId,
        string jwtId,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var entity = new RefreshToken
        {
            UserId = userId,
            JwtId = jwtId,
            FamilyId = Guid.NewGuid().ToString(),
            TokenHash = TokenHasher.Hash(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays),
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        };

        await _repo.AddAsync(entity);
        await _db.SaveChangesAsync();
        return rawToken;
    }

    public async Task<bool> ValidateAsync(string token)
    {
        var hash = TokenHasher.Hash(token);
        var stored = await _repo.GetByTokenHashAsync(hash);
        return stored is { IsActive: true };
    }

    public async Task RevokeAsync(
        string token,
        string? replacedByToken = null,
        string? revokedByIp = null)
    {
        var hash = TokenHasher.Hash(token);
        var stored = await _repo.GetByTokenHashAsync(hash);
        if (stored == null) return;

        stored.IsRevoked = true;
        stored.RevokedAtUtc = DateTime.UtcNow;
        stored.RevokedByIp = revokedByIp;
        _repo.Update(stored);
        await _db.SaveChangesAsync();
    }

    public async Task<(string Token, bool Compromised, Guid UserId)> RotateAsync(string token)
    {
        var hash = TokenHasher.Hash(token);

        // Use a transaction with a pessimistic row-level lock to prevent the
        // double-spend race condition where two concurrent requests both pass
        // the IsRevoked check before either write commits.
        await using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            // SELECT FOR UPDATE locks the row for the duration of the transaction.
            // Any concurrent request on the same token will block here until we commit.
            var existing = await _db.RefreshTokens
                .FromSqlRaw(
                    "SELECT * FROM \"RefreshTokens\" WHERE \"TokenHash\" = {0} FOR UPDATE",
                    hash)
                .FirstOrDefaultAsync();

            if (existing == null)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            // Token reuse detected — revoke entire family (security breach)
            if (existing.IsRevoked)
            {
                await _repo.RevokeFamilyAsync(existing.FamilyId);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return (string.Empty, true, Guid.Empty);
            }

            if (existing.ExpiresAtUtc <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token expired.");

            var newRaw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newHash = TokenHasher.Hash(newRaw);

            var newEntity = new RefreshToken
            {
                UserId = existing.UserId,
                JwtId = existing.JwtId,
                FamilyId = existing.FamilyId,
                TokenHash = newHash,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays)
            };

            existing.IsRevoked = true;
            existing.RevokedAtUtc = DateTime.UtcNow;
            existing.ReplacedByTokenHash = newHash;

            _repo.Update(existing);
            await _repo.AddAsync(newEntity);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return (newRaw, false, existing.UserId);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public string ComputeHash(string token) => TokenHasher.Hash(token);
}