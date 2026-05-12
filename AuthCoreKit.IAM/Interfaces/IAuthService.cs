using AuthCoreKit.IAM.Models;

namespace AuthCoreKit.IAM.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, Guid? tenantId);
        Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, Guid? tenantId);
        Task LogoutAsync(string refreshToken);
    }
}