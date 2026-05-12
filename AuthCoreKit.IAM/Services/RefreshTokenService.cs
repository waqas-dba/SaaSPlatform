using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;

namespace AuthCoreKit.IAM.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repo;
    private readonly JwtSettings _settings;

    public RefreshTokenService(IRefreshTokenRepository repo, IOptions<JwtSettings> options)
    {
        _repo = repo;
        _settings = options.Value;
    }

    public async Task<string> GenerateAsync(Guid userId, string jwtId,
        string? ipAddress = null, string? userAgent = null)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            JwtId = jwtId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays),
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        };
        await _repo.AddAsync(refreshToken);
        return token;
    }

    public async Task<bool> ValidateAsync(string token)
    {
        var rt = await _repo.GetByTokenAsync(token);
        return rt is { IsActive: true };
    }

    public async Task RevokeAsync(string token, string? replacedByToken = null, string? revokedByIp = null)
    {
        var rt = await _repo.GetByTokenAsync(token);
        if (rt == null) return;
        rt.IsRevoked = true;
        rt.RevokedAtUtc = DateTime.UtcNow;
        rt.ReplacedByToken = replacedByToken;
        rt.RevokedByIp = revokedByIp;
        await _repo.UpdateAsync(rt);
    }
}