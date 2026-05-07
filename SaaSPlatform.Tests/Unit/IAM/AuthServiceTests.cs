using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Core.IAM.Models;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.Infrastructure.Services.IAM;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SaaSPlatform.UnitTests.IAM;

public class AuthServiceTests
{
    private readonly IPasswordHasher _passwordHasher = new PasswordHasher();

    private IJwtTokenService CreateJwtService()
    {
        var settings = new JwtSettings
        {
            Secret = "SuperSecretKeyForTesting1234567890!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenMinutes = 5,
            RefreshTokenDays = 1
        };
        return new JwtTokenService(Options.Create(settings));
    }

    private SaaSPlatformDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<SaaSPlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SaaSPlatformDbContext(options);
    }

    private async Task<(SaaSPlatformDbContext db, User user, Guid tenantId)> SeedUserWithRole(string roleName)
    {
        var db = GetDbContext();
        var tenantId = Guid.NewGuid();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",                 // ← required
            Email = null,
            Phone = "123456789",
            PasswordHash = _passwordHasher.Hash("Password123"),
            IsActive = true
        };
        var role = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = roleName,
            IsSystem = false
        };
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            TenantId = tenantId
        };

        db.Users.Add(user);
        db.Roles.Add(role);
        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync();

        return (db, user, tenantId);
    }

    [Fact]
    public async Task Login_WithValidPhoneAndPassword_ReturnsTokens()
    {
        var (db, user, tenantId) = await SeedUserWithRole("Admin");
        var jwtService = CreateJwtService();
        var authService = new AuthService(db, _passwordHasher, jwtService);

        var request = new LoginRequest
        {
            Email = "123456789",
            Password = "Password123"
        };

        var response = await authService.LoginAsync(request, tenantId);

        Assert.NotNull(response);
        Assert.NotEmpty(response.AccessToken);
        Assert.NotEmpty(response.RefreshToken);

        var updatedUser = await db.Users.FindAsync(user.Id);
        Assert.NotNull(updatedUser!.LastLoginAt);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        var (db, _, tenantId) = await SeedUserWithRole("Admin");
        var authService = new AuthService(db, _passwordHasher, CreateJwtService());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => authService.LoginAsync(new LoginRequest { Email = "123456789", Password = "Wrong" }, tenantId));
    }

    [Fact]
    public async Task Login_WithNonExistentPhone_ThrowsUnauthorized()
    {
        var db = GetDbContext();
        var authService = new AuthService(db, _passwordHasher, CreateJwtService());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => authService.LoginAsync(new LoginRequest { Email = "0000", Password = "x" }, Guid.NewGuid()));
    }

    [Fact]
    public async Task Refresh_WithValidToken_RotatesTokens()
    {
        var (db, _, tenantId) = await SeedUserWithRole("Admin");
        var authService = new AuthService(db, _passwordHasher, CreateJwtService());

        var loginResponse = await authService.LoginAsync(
            new LoginRequest { Email = "123456789", Password = "Password123" }, tenantId);

        var refreshResponse = await authService.RefreshAsync(
            new RefreshTokenRequest { RefreshToken = loginResponse.RefreshToken }, tenantId);

        Assert.NotEmpty(refreshResponse.AccessToken);
        Assert.NotEqual(loginResponse.RefreshToken, refreshResponse.RefreshToken);

        var oldToken = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == loginResponse.RefreshToken);
        Assert.True(oldToken!.IsRevoked);
        Assert.Equal(refreshResponse.RefreshToken, oldToken.ReplacedByToken);
    }

    [Fact]
    public async Task Logout_RevokesToken()
    {
        var (db, _, tenantId) = await SeedUserWithRole("Admin");
        var authService = new AuthService(db, _passwordHasher, CreateJwtService());

        var loginResponse = await authService.LoginAsync(
            new LoginRequest { Email = "123456789", Password = "Password123" }, tenantId);

        await authService.LogoutAsync(loginResponse.RefreshToken);

        var token = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == loginResponse.RefreshToken);
        Assert.True(token!.IsRevoked);
        Assert.NotNull(token.RevokedAt);
    }
}