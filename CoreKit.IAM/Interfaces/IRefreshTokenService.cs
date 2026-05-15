namespace CoreKit.IAM.Interfaces;

/// <summary>
/// Service for generating, validating, and rotating refresh tokens.
/// </summary>
public interface IRefreshTokenService
{
    Task<string> GenerateAsync(Guid userId, string jwtId, string? ipAddress = null, string? userAgent = null);
    Task<bool> ValidateAsync(string token);
    Task RevokeAsync(string token, string? replacedByToken = null, string? revokedByIp = null);
    /// <summary>
    /// Rotates a refresh token. Returns the new token, whether the family was compromised, and the associated UserId.
    /// </summary>
    Task<(string Token, bool Compromised, Guid UserId)> RotateAsync(string token);
    string ComputeHash(string token);
}