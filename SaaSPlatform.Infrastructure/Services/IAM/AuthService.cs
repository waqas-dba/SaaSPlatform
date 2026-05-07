using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Core.IAM.Models;
using SaaSPlatform.Infrastructure.Persistence;

namespace SaaSPlatform.Infrastructure.Services.IAM;

public class AuthService : IAuthService
{
    private readonly SaaSPlatformDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        SaaSPlatformDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, Guid tenantId)
    {
        // 1. Find user by phone number (the username)
        var userRole = await _db.UserRoles
            .AsNoTracking()
            .Include(ur => ur.User)
            .FirstOrDefaultAsync(ur =>
                ur.TenantId == tenantId &&
                ur.User!.Phone == request.Email);   // ← phone is the login identifier

        if (userRole?.User is null)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var user = userRole.User;

        // 2. Verify password
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        // 3. Retrieve all roles for this user in the tenant
        var roles = await _db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == user.Id && ur.TenantId == tenantId)
            .Select(ur => ur.Role!.Name)
            .ToListAsync();

        // 4. Generate tokens (JWT already contains phone claim)
        var (accessToken, _) = _jwtTokenService.GenerateAccessToken(user, tenantId, roles);
        var refreshToken = CreateRefreshToken(user.Id);

        // 5. Update last login
        var trackedUser = await _db.Users.FindAsync(user.Id);
        trackedUser!.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, Guid tenantId)
    {
        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken is null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await _db.Users.FindAsync(storedToken.UserId);
        if (user is null) throw new UnauthorizedAccessException("User not found.");

        var roles = await _db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == user.Id && ur.TenantId == tenantId)
            .Select(ur => ur.Role!.Name)
            .ToListAsync();

        var (accessToken, _) = _jwtTokenService.GenerateAccessToken(user, tenantId, roles);

        // Rotate refresh token
        var newRefreshToken = CreateRefreshToken(user.Id);
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByToken = newRefreshToken.Token;

        await _db.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token != null)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    private RefreshToken CreateRefreshToken(Guid userId)
    {
        var token = new RefreshToken
        {
            UserId = userId,
            Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7) // ideally from JwtSettings
        };
        _db.RefreshTokens.Add(token);
        return token;
    }
}