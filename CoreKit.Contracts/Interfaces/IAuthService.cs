using CoreKit.Contracts.Models;   // request/response DTOs

namespace CoreKit.Contracts.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, Guid? tenantId);
    Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, Guid? tenantId);
    Task LogoutAsync(string refreshToken);
}