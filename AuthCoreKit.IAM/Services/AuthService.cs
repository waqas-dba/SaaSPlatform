using AuthCoreKit.IAM.Helpers;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthCoreKit.IAM.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly IPermissionService _permissionService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIamDbContext _db;
    private readonly IamOptions _options;

    public AuthService(
        IUserRepository userRepo,
        IJwtTokenService jwtService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshRepo,
        IPermissionService permissionService,
        IPasswordHasher passwordHasher,
        IIamDbContext db,
        IamOptions options)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _refreshRepo = refreshRepo;
        _permissionService = permissionService;
        _passwordHasher = passwordHasher;
        _db = db;
        _options = options;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, Guid? tenantId)
    {
        var user = await _userRepo.GetUserByLoginAsync(
            request.Login,
            tenantId,
            _options.LoginIdentifier);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is disabled.");
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException(
                $"Account locked until {user.LockoutEnd:O}");
        }

        var passwordValid = _passwordHasher.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }

            await _db.SaveChangesAsync();

            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var roles = user.Roles
            .Select(r => r.Role.Name)
            .Distinct()
            .ToList();

        var permissions = await _permissionService
            .GetUserPermissionsAsync(user.Id, tenantId);

        var (accessToken, _) = _jwtService.GenerateAccessToken(
            user,
            tenantId,
            roles,
            permissions);

        var refreshToken = await _refreshTokenService.GenerateAsync(
            user.Id,
            Guid.NewGuid().ToString());

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, Guid? tenantId)
    {
        var (newToken, compromised) =
            await _refreshTokenService.RotateAsync(request.RefreshToken);

        if (compromised)
            throw new UnauthorizedAccessException("Refresh token reuse detected. Session revoked.");

        var hash = _refreshTokenService.ComputeHash(request.RefreshToken);
        var stored = await _refreshRepo.GetByTokenHashAsync(hash);
        var user = await _userRepo.GetByIdAsync(stored!.UserId);

        var roles = user!.Roles.Select(r => r.Role.Name);

        var (accessToken, _) =
            _jwtService.GenerateAccessToken(user, tenantId, roles);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newToken
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _refreshTokenService.RevokeAsync(refreshToken);
    }

    
}