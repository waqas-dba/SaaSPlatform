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
        var previewHash = _refreshTokenService.ComputeHash(request.RefreshToken);
        var preview = await _db.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == previewHash, ct);

        if (preview == null || preview.IsRevoked || preview.ExpiresAtUtc <= DateTime.UtcNow)
        {
            // Even on an obviously invalid token attempt rotation to trigger family revocation
            // if the token was reused (the rotate service handles that internally).
            await _refreshTokenService.RotateAsync(request.RefreshToken, string.Empty);
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        // BUG FIX: GetByIdAsync in UserRepository includes Roles → Role, but does NOT
        // include RolePermissions → Permission. The cross-tenant check below accesses
        // role.RolePermissions.Any(...) which would always be empty without the include,
        // silently blocking platform admins from refreshing cross-tenant tokens.
        // Load with the full include chain here instead of relying on the repo method.
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
                    x.TenantId.HasValue && x.TenantId.Value == tenantId.Value);

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

    // ---------------------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------------------

    private List<string> BuildRoleList(User user, Guid? tenantId)
    {
        return user.Roles
            .Where(x => x.TenantId == null || x.TenantId == tenantId)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Returns the list of store IDs the user is assigned to within the given tenant,
    /// or null if no tenant context or no assignments exist (null = no store filter
    /// in the JWT, which is fine for platform-level users).
    /// </summary>
    private async Task<List<Guid>?> ResolveStoreIdsAsync(
        Guid userId,
        Guid? tenantId,
        CancellationToken ct)
    {
        if (!tenantId.HasValue)
            return null;

        // BUG FIX: original code returned null when count == 0, which is correct,
        // but the null check was done after the query in both LoginAsync and RefreshAsync
        // as duplicated inline code. Centralised here to a single private method.
        var storeIds = await _db.Set<UserStoreAssignment>()
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.TenantId == tenantId.Value)
            .Select(x => x.StoreId)
            .ToListAsync(ct);

        return storeIds.Count > 0 ? storeIds : null;
    }
}