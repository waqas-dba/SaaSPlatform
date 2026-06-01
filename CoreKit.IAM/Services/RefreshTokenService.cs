// CoreKit.IAM/Services/RefreshTokenService.cs
using CoreKit.IAM.Entities;
using CoreKit.IAM.Helpers;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
using Microsoft.EntityFrameworkCore;
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

    public async Task<(string NewRefreshToken, Guid UserId)> RotateAsync(
        string token,
        string newJwtId)
    {
        var hash = TokenHasher.Hash(token);
        await using var tx = await _db.Database.BeginTransactionAsync();

        // Track whether we committed so the catch block does not attempt
        // a rollback on an already-completed transaction.
        var committed = false;

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
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            // FIX: Distinguish between a replayed (already-revoked) token and
            // a simply expired token. Only replay triggers family revocation —
            // a security response to token theft. An expired token is a normal
            // session-end condition that should not cascade-revoke other sessions
            // the user may have open on different devices.
            if (existing.IsRevoked)
            {
                _logger.LogWarning(
                    "Refresh token reuse detected. " +
                    "UserId={UserId} FamilyId={FamilyId} RevokedAt={RevokedAt} " +
                    "— revoking all family tokens.",
                    existing.UserId,
                    existing.FamilyId,
                    existing.RevokedAtUtc);

                await _repo.RevokeFamilyAsync(existing.FamilyId);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                committed = true;

                throw new UnauthorizedAccessException(
                    "Refresh token reuse detected. " +
                    "All sessions have been revoked for security.");
            }

            // FIX: Check expiry separately — expired tokens are simply rejected,
            // no family revocation. Previously the AuthService called RotateAsync
            // even for expired tokens, which would reach here and potentially
            // cascade-revoke all user sessions unnecessarily.
            if (existing.ExpiresAtUtc <= DateTime.UtcNow)
            {
                await tx.RollbackAsync();
                throw new UnauthorizedAccessException(
                    "Refresh token has expired. Please log in again.");
            }

            var newRaw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
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
            committed = true;

            return (newRaw, existing.UserId);
        }
        catch (UnauthorizedAccessException)
        {
            if (!committed)
            {
                try { await tx.RollbackAsync(); }
                catch (Exception rollbackEx)
                {
                    _logger.LogWarning(rollbackEx,
                        "Rollback failed — transaction may already be completed.");
                }
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error during token rotation for hash {Hash}", hash);

            if (!committed)
            {
                try { await tx.RollbackAsync(); }
                catch (Exception rollbackEx)
                {
                    _logger.LogWarning(rollbackEx,
                        "Rollback failed after unexpected error.");
                }
            }
            throw;
        }
    }

    public string ComputeHash(string token) => TokenHasher.Hash(token);
}