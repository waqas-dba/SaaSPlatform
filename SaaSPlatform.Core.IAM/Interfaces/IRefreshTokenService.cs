namespace SaaSPlatform.Core.IAM.Interfaces;

public interface IRefreshTokenService
{
    Task<string> GenerateAsync(
        Guid userId,
        string jwtId,
        string? ipAddress = null,
        string? userAgent = null);

    Task<bool> ValidateAsync(string token);

    Task RevokeAsync(
        string token,
        string? replacedByToken = null,
        string? revokedByIp = null);
}