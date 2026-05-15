using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;

namespace CoreKit.IAM.Services;

/// <summary>
/// Authentication service handling login, token refresh, and logout.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IPermissionService _permissionService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IamDbContext _db;
    private readonly IamOptions _options;

    public AuthService(
        IUserRepository userRepo,
        IJwtTokenService jwtService,
        IRefreshTokenService refreshTokenService,
        IPermissionService permissionService,
        IPasswordHasher passwordHasher,
        IamDbContext db,
        IamOptions options)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _permissionService = permissionService;
        _passwordHasher = passwordHasher;
        _db = db;
        _options = options;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, Guid? tenantId)
    {
        var user = await _userRepo.GetUserByLoginAsync(request.Login, tenantId, _options.LoginIdentifier)
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedAccessException($"Account locked until {user.LockoutEnd:O}");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            await _db.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var roles = user.Roles.Select(r => r.Role.Name).Distinct().ToList();
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id, tenantId);
        var (accessToken, _) = _jwtService.GenerateAccessToken(user, tenantId, roles, permissions);
        var refreshToken = await _refreshTokenService.GenerateAsync(user.Id, Guid.NewGuid().ToString());

        return new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, Guid? tenantId)
    {
        var (newToken, compromised, userId) = await _refreshTokenService.RotateAsync(request.RefreshToken);

        if (compromised)
            throw new UnauthorizedAccessException("Refresh token reuse detected. Session revoked.");

        var user = await _userRepo.GetByIdAsync(userId);
        var roles = user!.Roles.Select(r => r.Role.Name);
        var (accessToken, _) = _jwtService.GenerateAccessToken(user, tenantId, roles);

        return new LoginResponse { AccessToken = accessToken, RefreshToken = newToken };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _refreshTokenService.RevokeAsync(refreshToken);
    }
}