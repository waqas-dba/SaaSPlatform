using Microsoft.EntityFrameworkCore;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;

namespace CoreKit.IAM.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IPermissionService _permissionService;
    private readonly IPermissionCacheService _permissionCache;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IamDbContext _db;
    private readonly IamOptions _options;

    public AuthService(
        IUserRepository userRepo,
        IJwtTokenService jwtService,
        IRefreshTokenService refreshTokenService,
        IPermissionService permissionService,
        IPermissionCacheService permissionCache,
        IPasswordHasher passwordHasher,
        IamDbContext db,
        IamOptions options)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _permissionService = permissionService;
        _permissionCache = permissionCache;
        _passwordHasher = passwordHasher;
        _db = db;
        _options = options;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, Guid? tenantId)
    {
        var user = await _userRepo.GetUserByLoginAsync(
            request.Login, tenantId, _options.LoginIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedAccessException(
                $"Account locked until {user.LockoutEnd:O}");

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

        var roles = BuildRoleList(user, tenantId);

        // Always fetch fresh permissions at login — bypass cache
        await _permissionCache.InvalidateAsync(user.Id, tenantId);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id, tenantId);

        List<Guid>? storeIds = null;
        if (tenantId.HasValue)
        {
            storeIds = await _db.Set<UserStoreAssignment>()
                .Where(a => a.UserId == user.Id && a.TenantId == tenantId.Value)
                .Select(a => a.StoreId)
                .ToListAsync();

            if (storeIds.Count == 0) storeIds = null;
        }

        var (accessToken, _, jwtId) = _jwtService.GenerateAccessToken(
            user, tenantId, roles, permissions, storeIds);

        var refreshToken = await _refreshTokenService.GenerateAsync(user.Id, jwtId);

        return new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, Guid? tenantId)
    {
        var (newToken, compromised, userId) =
            await _refreshTokenService.RotateAsync(request.RefreshToken);

        if (compromised)
            throw new UnauthorizedAccessException(
                "Refresh token reuse detected. Session revoked.");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        var isSuperAdmin = user.Roles.Any(r =>
            r.Role.Name == _options.SuperAdminRoleName);

        if (tenantId.HasValue && !isSuperAdmin)
        {
            bool belongsToTenant = user.Roles.Any(ur =>
                ur.TenantId.HasValue && ur.TenantId.Value == tenantId.Value);

            if (!belongsToTenant)
                throw new UnauthorizedAccessException(
                    "User does not belong to this tenant.");
        }

        var roles = BuildRoleList(user, tenantId);

        // Always invalidate cache on token refresh — permissions may have changed
        // since the last login. This ensures a demoted user never keeps elevated
        // permissions past the next refresh cycle.
        await _permissionCache.InvalidateAsync(user.Id, tenantId);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id, tenantId);

        List<Guid>? storeIds = null;
        if (tenantId.HasValue)
        {
            storeIds = await _db.Set<UserStoreAssignment>()
                .Where(a => a.UserId == user.Id && a.TenantId == tenantId.Value)
                .Select(a => a.StoreId)
                .ToListAsync();

            if (storeIds.Count == 0) storeIds = null;
        }

        var (accessToken, _, _) = _jwtService.GenerateAccessToken(
            user, tenantId, roles, permissions, storeIds);

        return new LoginResponse { AccessToken = accessToken, RefreshToken = newToken };
    }

    public async Task LogoutAsync(string refreshToken)
        => await _refreshTokenService.RevokeAsync(refreshToken);

    private List<string> BuildRoleList(User user, Guid? tenantId)
        => user.Roles
            .Where(r =>
                r.TenantId == null ||
                r.TenantId == tenantId)
            .Select(r => r.Role.Name)
            .Distinct()
            .ToList();
}