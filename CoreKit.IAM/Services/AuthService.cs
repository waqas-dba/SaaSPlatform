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

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        Guid? tenantId,
        CancellationToken ct = default)
    {
        var user = await _userRepo.GetUserByLoginAsync(
            request.Login, tenantId, _options.LoginIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        ValidateUserAccess(user);

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await RecordFailedLoginAsync(user, ct);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await BuildLoginResponseAsync(user, tenantId, ct);
    }

    public async Task<LoginResponse> RefreshAsync(
        RefreshTokenRequest request,
        Guid? tenantId,
        CancellationToken ct = default)
    {
        // FIX: removed the unlocked preview check — all validation now
        // happens inside RotateAsync under SELECT FOR UPDATE
        var (newRefreshToken, userId) =
            await _refreshTokenService.RotateAsync(request.RefreshToken, string.Empty);

        var user = await _db.Users
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        if (tenantId.HasValue)
            ValidateTenantAccess(user, tenantId.Value);

        var roles = BuildRoleList(user, tenantId);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id, tenantId);
        var storeIds = await ResolveStoreIdsAsync(user.Id, tenantId, ct);

        var (accessToken, _, newJwtId) = _jwtService.GenerateAccessToken(
            user, tenantId, roles, permissions, storeIds);

        // Rotate again with the real jwtId now that we have it
        var (finalRefreshToken, _) = await _refreshTokenService.RotateAsync(
            newRefreshToken, newJwtId);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = finalRefreshToken
        };
    }

    public async Task LogoutAsync(string refreshToken)
        => await _refreshTokenService.RevokeAsync(refreshToken);

    // --- private helpers ---

    private static void ValidateUserAccess(User user)
    {
        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedAccessException(
                $"Account locked until {user.LockoutEnd:O}");
    }

    private async Task RecordFailedLoginAsync(User user, CancellationToken ct)
    {
        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts >= 5)
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateTenantAccess(User user, Guid tenantId)
    {
        var hasPlatformAccess = user.Roles.Any(x =>
            x.TenantId == null &&
            x.Role.RolePermissions.Any(rp =>
                rp.Permission.Name == Permissions.Platform.ViewAllTenants));

        if (hasPlatformAccess) return;

        var belongsToTenant = user.Roles.Any(x =>
            x.TenantId.HasValue && x.TenantId.Value == tenantId);

        if (!belongsToTenant)
            throw new UnauthorizedAccessException(
                "User does not belong to this tenant.");
    }

    private async Task<LoginResponse> BuildLoginResponseAsync(
        User user, Guid? tenantId, CancellationToken ct)
    {
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

    private static List<string> BuildRoleList(User user, Guid? tenantId)
        => user.Roles
            .Where(x => x.TenantId == null || x.TenantId == tenantId)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToList();

    private async Task<List<Guid>?> ResolveStoreIdsAsync(
        Guid userId, Guid? tenantId, CancellationToken ct)
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