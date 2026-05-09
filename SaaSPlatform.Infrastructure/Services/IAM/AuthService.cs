using BCrypt.Net;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Core.IAM.Models;

namespace SaaSPlatform.Core.IAM.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtService;
        private readonly IRefreshTokenRepository _refreshRepo;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtService,
            IRefreshTokenRepository refreshRepo)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _refreshRepo = refreshRepo;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, Guid tenantId)
        {
            var user = await _userRepository.GetByPhoneAsync(request.Phone, tenantId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            var roles = user.Roles.Select(x => x.Role.Name).ToList();
            var (accessToken, _) = _jwtService.GenerateAccessToken(user, tenantId, roles);

            var refresh = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString("N"),   // For production, use RefreshTokenService
                JwtId = Guid.NewGuid().ToString(),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            };
            await _refreshRepo.AddAsync(refresh);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refresh.Token
            };
        }

        public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, Guid tenantId)
        {
            var storedToken = await _refreshRepo.GetByTokenAsync(request.RefreshToken);
            if (storedToken == null || !storedToken.IsActive)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            var user = await _userRepository.GetByIdAsync(storedToken.UserId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            // Rotate the refresh token
            await _refreshRepo.RevokeAsync(storedToken);

            var roles = user.Roles.Select(x => x.Role.Name).ToList();
            var (newAccessToken, _) = _jwtService.GenerateAccessToken(user, tenantId, roles);

            var newRefresh = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString("N"),
                JwtId = Guid.NewGuid().ToString(),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            };
            await _refreshRepo.AddAsync(newRefresh);

            return new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefresh.Token
            };
        }

        public Task LogoutAsync(string refreshToken)
        {
            // TODO: implement revocation
            throw new NotImplementedException();
        }
    }
}