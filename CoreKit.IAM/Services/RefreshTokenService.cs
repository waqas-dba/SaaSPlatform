using System.Security.Cryptography;
using System.Text;
using CoreKit.IAM.Entities;
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

    private static string Hash(string token)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
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
            TokenHash = Hash(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays),
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        };

        await _repo.AddAsync(entity);
        return rawToken;
    }

    public async Task<bool> ValidateAsync(string token)
    {
        var hash = Hash(token);
        var stored = await _repo.GetByTokenHashAsync(hash);
        return stored is { IsActive: true };
    }

    public async Task RevokeAsync(
        string token,
        string? replacedByToken = null,
        string? revokedByIp = null)
    {
        var hash = Hash(token);
        var stored = await _repo.GetByTokenHashAsync(hash);

        if (stored == null) return;

        stored.IsRevoked = true;
        stored.RevokedAtUtc = DateTime.UtcNow;
        stored.RevokedByIp = revokedByIp;

        await _repo.UpdateAsync(stored);
    }

    // 🔥 CORE: ROTATION + THEFT DETECTION
    public async Task<(string Token, bool Compromised)> RotateAsync(string token)
    {
        var hash = Hash(token);
        var existing = await _repo.GetByTokenHashAsync(hash);

        if (existing == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // 🚨 reuse detection (token already revoked but used again)
        if (existing.IsRevoked)
        {
            await _repo.RevokeFamilyAsync(existing.FamilyId);
            return (string.Empty, true);
        }

        var newToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var newEntity = new RefreshToken
        {
            UserId = existing.UserId,
            JwtId = existing.JwtId,
            FamilyId = existing.FamilyId,
            TokenHash = Hash(newToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays)
        };

        // revoke old
        existing.IsRevoked = true;
        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedByTokenHash = newEntity.TokenHash;

        await _repo.UpdateAsync(existing);
        await _repo.AddAsync(newEntity);

        return (newToken, false);
    }

    public string ComputeHash(string token)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(
            sha.ComputeHash(Encoding.UTF8.GetBytes(token))
        );
    }
}