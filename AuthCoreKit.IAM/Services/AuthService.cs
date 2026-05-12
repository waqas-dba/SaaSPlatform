using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;

namespace AuthCoreKit.IAM.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IamOptions _options;                     // Added

    public AuthService(
        IUserRepository userRepo,
        IJwtTokenService jwtService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshRepo,
        IPasswordHasher passwordHasher,
        IamOptions options)                                   // Added
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _refreshRepo = refreshRepo;
        _passwordHasher = passwordHasher;
        _options = options;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, Guid? tenantId)
    {
        // Use the configurable login identifier
        var user = await _userRepo.GetUserByLoginAsync(request.Login, tenantId, _options.LoginIdentifier);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var roles = user.Roles.Select(r => r.Role.Name).ToList();
        var (accessToken, _) = _jwtService.GenerateAccessToken(user, tenantId, roles);
        var refreshToken = await _refreshTokenService.GenerateAsync(user.Id, Guid.NewGuid().ToString());

        return new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, Guid? tenantId)
    {
        var storedToken = await _refreshRepo.GetByTokenAsync(request.RefreshToken);
        if (storedToken == null || !storedToken.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = await _userRepo.GetByIdAsync(storedToken.UserId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        await _refreshTokenService.RevokeAsync(storedToken.Token);

        var roles = user.Roles.Select(r => r.Role.Name).ToList();
        var (newAccessToken, _) = _jwtService.GenerateAccessToken(user, tenantId, roles);
        var newRefresh = await _refreshTokenService.GenerateAsync(user.Id, Guid.NewGuid().ToString());

        return new LoginResponse { AccessToken = newAccessToken, RefreshToken = newRefresh };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _refreshTokenService.RevokeAsync(refreshToken);
    }
}