// CoreKit.IAM/Services/AuthService.cs
using CoreKit.IAM.Constants;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        Guid? tenantId,
        CancellationToken ct = default)
    {
        var user = await _userRepo.GetUserByLoginAsync(
            request.Login,
            tenantId,
            _options.LoginIdentifier)
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
            await _db.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var roles = BuildRoleList(user, tenantId);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id, tenantId);
        var storeIds = await ResolveStoreIdsAsync(user.Id, tenantId, ct);

        var (accessToken, _, jwtId) = _jwtService.GenerateAccessToken(
            user, tenantId, roles, permissions, storeIds);

        var refreshToken = await _refreshTokenService.GenerateAsync(user.Id, jwtId);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<LoginResponse> RefreshAsync(
        RefreshTokenRequest request,
        Guid? tenantId,
        CancellationToken ct = default)
    {
        // FIX: Pre-flight check now only rejects clearly invalid tokens
        // (not found, or already revoked). Expiry is NOT checked here.
        // RotateAsync handles expiry separately so an expired-but-valid
        // token does not trigger family revocation via the old code path
        // of calling RotateAsync(token, string.Empty).
        var previewHash = _refreshTokenService.ComputeHash(request.RefreshToken);
        var preview = await _db.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == previewHash, ct);

        if (preview == null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        // FIX: If already revoked, call RotateAsync to trigger reuse detection
        // and family revocation. Do not call it for expired tokens.
        if (preview.IsRevoked)
        {
            // This will detect reuse and revoke the family inside a transaction.
            await _refreshTokenService.RotateAsync(request.RefreshToken, string.Empty);
            throw new UnauthorizedAccessException(
                "Refresh token reuse detected. All sessions have been revoked.");
        }

        // Let RotateAsync handle expiry check — it will throw cleanly without
        // triggering family revocation.
        var user = await _db.Users
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == preview.UserId, ct)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        if (tenantId.HasValue)
        {
            var hasPlatformAccess = user.Roles.Any(x =>
                x.TenantId == null &&
                x.Role.RolePermissions.Any(rp =>
                    rp.Permission.Name == Permissions.Platform.ViewAllTenants));

            if (!hasPlatformAccess)
            {
                var belongsToTenant = user.Roles.Any(x =>
                    x.TenantId.HasValue &&
                    x.TenantId.Value == tenantId.Value);

                if (!belongsToTenant)
                    throw new UnauthorizedAccessException(
                        "User does not belong to this tenant.");
            }
        }

        var roles = BuildRoleList(user, tenantId);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id, tenantId);
        var storeIds = await ResolveStoreIdsAsync(user.Id, tenantId, ct);

        var (accessToken, _, newJwtId) = _jwtService.GenerateAccessToken(
            user, tenantId, roles, permissions, storeIds);

        // RotateAsync will throw UnauthorizedAccessException if the token
        // is expired — no family revocation occurs in that path.
        var (newRefreshToken, _) = await _refreshTokenService.RotateAsync(
            request.RefreshToken, newJwtId);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _refreshTokenService.RevokeAsync(refreshToken);
    }

    private List<string> BuildRoleList(User user, Guid? tenantId)
    {
        return user.Roles
            .Where(x => x.TenantId == null || x.TenantId == tenantId)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToList();
    }

    private async Task<List<Guid>?> ResolveStoreIdsAsync(
        Guid userId,
        Guid? tenantId,
        CancellationToken ct)
    {
        if (!tenantId.HasValue) return null;

        var storeIds = await _db.Set<UserStoreAssignment>()
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.TenantId == tenantId.Value)
            .Select(x => x.StoreId)
            .ToListAsync(ct);

        return storeIds.Count > 0 ? storeIds : null;
    }
}