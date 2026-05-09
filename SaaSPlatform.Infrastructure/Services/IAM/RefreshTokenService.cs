using System.Security.Cryptography;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using Microsoft.Extensions.Options;

namespace SaaSPlatform.Infrastructure.Services.Auth;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repository;

    private readonly JwtSettings _jwtSettings;

    public RefreshTokenService(
        IRefreshTokenRepository repository,
        IOptions<JwtSettings> jwtOptions)
    {
        _repository = repository;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<string> GenerateAsync(
        Guid userId,
        string jwtId,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var token = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            JwtId = jwtId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenDays),

            CreatedAtUtc = DateTime.UtcNow,

            CreatedByIp = ipAddress,

            UserAgent = userAgent
        };

        await _repository.AddAsync(refreshToken);

        await _repository.SaveChangesAsync();

        return token;
    }

    public async Task<bool> ValidateAsync(string token)
    {
        var refreshToken = await _repository
            .GetByTokenAsync(token);

        return refreshToken != null &&
               refreshToken.IsActive;
    }

    public async Task RevokeAsync(
        string token,
        string? replacedByToken = null,
        string? revokedByIp = null)
    {
        var refreshToken = await _repository
            .GetByTokenAsync(token);

        if (refreshToken == null)
            return;

        refreshToken.IsRevoked = true;

        refreshToken.RevokedAtUtc = DateTime.UtcNow;

        refreshToken.ReplacedByToken = replacedByToken;

        refreshToken.RevokedByIp = revokedByIp;

        await _repository.SaveChangesAsync();
    }
}