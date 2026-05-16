// CoreKit.IAM/Services/RefreshTokenService.cs
using System.Security.Cryptography;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Helpers;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using Microsoft.Extensions.Options;

namespace CoreKit.IAM.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repo;
    private readonly JwtSettings _settings;

    public RefreshTokenService(
        IRefreshTokenRepository repo,
        IOptions<JwtSettings> options)
    {
        _repo = repo;
        _settings = options.Value;
    }

    public async Task<string> GenerateAsync(
        Guid userId,
        string jwtId,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var familyId = Guid.NewGuid().ToString();

        var entity = new RefreshToken
        {
            UserId = userId,
            JwtId = jwtId,
            FamilyId = familyId,
            TokenHash = TokenHasher.Hash(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays),
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        };

        await _repo.AddAsync(entity);
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
        await _repo.UpdateAsync(stored);
    }

    public async Task<(string Token, bool Compromised, Guid UserId)> RotateAsync(string token)
    {
        var hash = TokenHasher.Hash(token);
        var existing = await _repo.GetByTokenHashAsync(hash)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        // FIX: Check revocation BEFORE expiry.
        // A token that is both revoked and expired is a reuse/theft signal —
        // the correct response is family revocation, not an "expired" error
        // which would hide the compromise.
        if (existing.IsRevoked)
        {
            await _repo.RevokeFamilyAsync(existing.FamilyId);
            return (string.Empty, true, Guid.Empty);
        }

        if (existing.ExpiresAtUtc <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired.");

        var newToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var newEntity = new RefreshToken
        {
            UserId = existing.UserId,
            JwtId = existing.JwtId,
            FamilyId = existing.FamilyId,
            TokenHash = TokenHasher.Hash(newToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays)
        };

        existing.IsRevoked = true;
        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedByTokenHash = newEntity.TokenHash;

        await _repo.UpdateAsync(existing);
        await _repo.AddAsync(newEntity);

        return (newToken, false, existing.UserId);
    }

    public string ComputeHash(string token) => TokenHasher.Hash(token);
}