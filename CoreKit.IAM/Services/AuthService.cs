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

        // Roles scoped to the login tenant (or system‑level SuperAdmin)
        var roles = user.Roles
            .Where(r =>
                r.TenantId == tenantId ||                         // roles granted in this tenant
                r.Role.Name == _options.SuperAdminRoleName)       // system‑level roles
            .Select(r => r.Role.Name)
            .Distinct()
            .ToList();

        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id, tenantId);

        // Retrieve store assignments for the current tenant
        List<Guid>? storeIds = null;
        if (tenantId.HasValue)
        {
            storeIds = await _db.Set<UserStoreAssignment>()
                .Where(a => a.UserId == user.Id && a.TenantId == tenantId.Value)
                .Select(a => a.StoreId)
                .ToListAsync();

            if (storeIds.Count == 0) storeIds = null;   // null means no store claims
        }

        var (accessToken, _) = _jwtService.GenerateAccessToken(
            user, tenantId, roles, permissions, storeIds);

        var refreshToken = await _refreshTokenService.GenerateAsync(user.Id, Guid.NewGuid().ToString());

        return new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, Guid? tenantId)
    {
        var (newToken, compromised, userId) = await _refreshTokenService.RotateAsync(request.RefreshToken);
        if (compromised)
            throw new UnauthorizedAccessException("Refresh token reuse detected. Session revoked.");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found.");

        // Tenant membership check
        if (tenantId.HasValue)
        {
            bool belongsToTenant = user.Roles.Any(ur => ur.TenantId == tenantId);
            if (!belongsToTenant)
                throw new UnauthorizedAccessException("User does not belong to this tenant.");
        }

        // Roles scoped to the tenant (same logic as LoginAsync)
        var roles = user.Roles
            .Where(r =>
                r.TenantId == tenantId ||
                r.Role.Name == _options.SuperAdminRoleName)
            .Select(r => r.Role.Name)
            .Distinct()
            .ToList();

        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id, tenantId);

        // Retrieve store assignments for the refreshed token
        List<Guid>? storeIds = null;
        if (tenantId.HasValue)
        {
            storeIds = await _db.Set<UserStoreAssignment>()
                .Where(a => a.UserId == user.Id && a.TenantId == tenantId.Value)
                .Select(a => a.StoreId)
                .ToListAsync();

            if (storeIds.Count == 0) storeIds = null;
        }

        var (accessToken, _) = _jwtService.GenerateAccessToken(
            user, tenantId, roles, permissions, storeIds);

        return new LoginResponse { AccessToken = accessToken, RefreshToken = newToken };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _refreshTokenService.RevokeAsync(refreshToken);
    }
}