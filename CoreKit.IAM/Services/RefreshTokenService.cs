using CoreKit.IAM.Entities;
using CoreKit.IAM.Helpers;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace CoreKit.IAM.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repo;
    private readonly IamDbContext _db;
    private readonly JwtSettings _settings;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IRefreshTokenRepository repo,
        IamDbContext db,
        IOptions<JwtSettings> options,
        ILogger<RefreshTokenService> logger)
    {
        _repo = repo;
        _db = db;
        _settings = options.Value;
        _logger = logger;
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
        if (replacedByToken != null)
            stored.ReplacedByTokenHash = TokenHasher.Hash(replacedByToken);

        _repo.Update(stored);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Rotates the refresh token.
    ///
    /// CRITICAL FIX 1 — the new token uses newJwtId (the freshly-minted
    ///   access token's JWT ID), not the original expired jwtId.
    ///
    /// CRITICAL FIX 2 — a compromised (already-revoked) token revokes the
    ///   whole family and throws immediately instead of returning a sentinel
    ///   tuple that a caller might mishandle.
    /// </summary>
    public async Task<(string NewRefreshToken, Guid UserId)> RotateAsync(
    string token,
    string newJwtId)
    {
        var hash = TokenHasher.Hash(token);

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var existing = await _db.RefreshTokens
                .FromSqlRaw(
                    "SELECT * FROM \"RefreshTokens\" " +
                    "WHERE \"TokenHash\" = {0} FOR UPDATE",
                    hash)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                await tx.RollbackAsync();
                throw new UnauthorizedAccessException(
                    "Invalid refresh token.");
            }

            if (existing.IsRevoked)
            {
                // Security event: token reuse detected — revoke entire family
                _logger.LogWarning(
                    "Refresh token reuse detected. " +
                    "UserId={UserId} FamilyId={FamilyId} " +
                    "RevokedAt={RevokedAt} — revoking all family tokens.",
                    existing.UserId,
                    existing.FamilyId,
                    existing.RevokedAtUtc);

                await _repo.RevokeFamilyAsync(existing.FamilyId);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                throw new UnauthorizedAccessException(
                    "Refresh token reuse detected. " +
                    "All sessions have been revoked for security.");
            }

            if (existing.ExpiresAtUtc <= DateTime.UtcNow)
            {
                await tx.RollbackAsync();
                throw new UnauthorizedAccessException(
                    "Refresh token has expired. Please log in again.");
            }

            var newRaw = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));
            var newHash = TokenHasher.Hash(newRaw);

            var newEntity = new RefreshToken
            {
                UserId = existing.UserId,
                JwtId = newJwtId,
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

            return (newRaw, existing.UserId);
        }
        catch (UnauthorizedAccessException)
        {
            // Already handled above — rollback only if tx still open
            if (tx.GetDbTransaction().Connection != null)
                await tx.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error during token rotation for hash {Hash}", hash);
            await tx.RollbackAsync();
            throw;
        }
    }

    public string ComputeHash(string token) => TokenHasher.Hash(token);
}