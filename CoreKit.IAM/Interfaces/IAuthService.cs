using CoreKit.IAM.Models;

namespace CoreKit.IAM.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        Guid? tenantId,
        CancellationToken ct = default);

    Task<LoginResponse> RefreshAsync(
        RefreshTokenRequest request,
        Guid? tenantId,
        CancellationToken ct = default);

    Task LogoutAsync(string refreshToken);
}