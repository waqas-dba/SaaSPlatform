using SaaSPlatform.Core.IAM.Models;

namespace SaaSPlatform.Core.IAM.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, Guid tenantId);
    Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, Guid tenantId);
    Task LogoutAsync(string refreshToken);
}