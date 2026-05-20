namespace CoreKit.IAM.Interfaces;

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

    /// <summary>
    /// Rotates the given refresh token. Accepts the jwtId from the
    /// freshly-generated access token so the new refresh token binds
    /// to the correct JWT — not the original expired one.
    /// Throws UnauthorizedAccessException on compromise or expiry.
    /// </summary>
    Task<(string NewRefreshToken, Guid UserId)> RotateAsync(
        string token,
        string newJwtId);

    string ComputeHash(string token);
}