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

        // HIGH FIX — do NOT invalidate before fetching; that creates a race.
        // Permissions are fetched fresh here; cache is populated as a side-effect
        // of GetUserPermissionsAsync. Invalidation belongs only when permissions change.
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
        // CRITICAL FIX — generate the access token FIRST so its jwtId can be
        // passed into RotateAsync. The old code passed the stale original jwtId.
        // We need the user to build the token, so fetch them before rotating.

        // Step 1: validate the incoming token enough to get the userId without rotating yet
        var previewHash = _refreshTokenService.ComputeHash(request.RefreshToken);
        var preview = await _db.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == previewHash);

        if (preview == null || preview.IsRevoked || preview.ExpiresAtUtc <= DateTime.UtcNow)
        {
            // If it is revoked we still want to trigger the family revocation,
            // so let RotateAsync handle it and re-throw.
            // Pass a placeholder jwtId — RotateAsync will throw before using it.
            await _refreshTokenService.RotateAsync(request.RefreshToken, string.Empty);
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user = await _userRepo.GetByIdAsync(preview.UserId)
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

        // Step 2: generate the new access token — its jwtId binds to the new refresh token
        var (accessToken, _, newJwtId) = _jwtService.GenerateAccessToken(
            user, tenantId, roles, permissions, storeIds);

        // Step 3: rotate, passing in the new jwtId so the rotated token is bound correctly
        var (newRefreshToken, _) = await _refreshTokenService.RotateAsync(
            request.RefreshToken, newJwtId);

        return new LoginResponse { AccessToken = accessToken, RefreshToken = newRefreshToken };
    }

    public async Task LogoutAsync(string refreshToken)
        => await _refreshTokenService.RevokeAsync(refreshToken);

    private List<string> BuildRoleList(User user, Guid? tenantId)
        => user.Roles
            .Where(r => r.TenantId == null || r.TenantId == tenantId)
            .Select(r => r.Role.Name)
            .Distinct()
            .ToList();
}