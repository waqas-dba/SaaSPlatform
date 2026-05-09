namespace SaaSPlatform.Application.DTOs.Auth;

public class AuthTokenResponse
{
    public string AccessToken { get; set; } = default!;

    public DateTime AccessTokenExpiresAtUtc { get; set; }

    public string RefreshToken { get; set; } = default!;

    public DateTime RefreshTokenExpiresAtUtc { get; set; }
}