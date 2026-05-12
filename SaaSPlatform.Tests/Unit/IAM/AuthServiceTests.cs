using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;
using AuthCoreKit.IAM.Services;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.Infrastructure.Services;
using SaaSPlatform.Infrastructure.Services.IAM;
using SaaSPlatform.UnitTests.Fakes;
using Xunit;

namespace SaaSPlatform.UnitTests.IAM;

public class AuthServiceTests
{
    private readonly IPasswordHasher _passwordHasher =
        new PasswordHasher();

    // =====================================================
    // JWT SERVICE
    // =====================================================

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

        return new JwtTokenService(
            Options.Create(settings));
    }

    // =====================================================
    // DB CONTEXT
    // =====================================================

    private SaaSPlatformDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<SaaSPlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SaaSPlatformDbContext(
            options,
            tenantContext: null,
            currentUser: null);
    }

    // =====================================================
    // SEED USER
    // =====================================================

    private async Task<(
        SaaSPlatformDbContext db,
        IUserRepository userRepository,
        Guid tenantId)>
        SeedUser()
    {
        var db = GetDbContext();

        var tenantId = Guid.NewGuid();

        var user = new User
        {
            Id = Guid.NewGuid(),

            Name = "Test User",

            Email = null,

            Phone = "123456789",

            PasswordHash = _passwordHasher.Hash("Password123"),

            IsActive = true
        };

        db.Users.Add(user);

        await db.SaveChangesAsync();

        var repo = new UserRepository(db);

        return (db, repo, tenantId);
    }

    // =====================================================
    // LOGIN SUCCESS
    // =====================================================

    [Fact]
    public async Task Login_WithValidPhoneAndPassword_ReturnsTokens()
    {
        // Arrange
        var (_, repo, tenantId) = await SeedUser();

        var authService = new AuthService(
            repo,
            CreateJwtService(),
            new FakeRefreshTokenRepository(),
            _passwordHasher);

        var request = new LoginRequest
        {
            Phone = "123456789",
            Password = "Password123"
        };

        // Act
        var response =
            await authService.LoginAsync(request, tenantId);

        // Assert
        Assert.NotNull(response);

        Assert.NotEmpty(response.AccessToken);

        Assert.NotEmpty(response.RefreshToken);
    }

    // =====================================================
    // WRONG PASSWORD
    // =====================================================

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        // Arrange
        var (_, repo, tenantId) = await SeedUser();

        var authService = new AuthService(
            repo,
            CreateJwtService(),
            new FakeRefreshTokenRepository(),
            _passwordHasher);

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => authService.LoginAsync(
                new LoginRequest
                {
                    Phone = "123456789",
                    Password = "WrongPassword"
                },
                tenantId));
    }

    // =====================================================
    // UNKNOWN PHONE
    // =====================================================

    [Fact]
    public async Task Login_WithUnknownPhone_ThrowsUnauthorized()
    {
        // Arrange
        var db = GetDbContext();

        var repo = new UserRepository(db);

        var authService = new AuthService(
            repo,
            CreateJwtService(),
            new FakeRefreshTokenRepository(),
            _passwordHasher);

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => authService.LoginAsync(
                new LoginRequest
                {
                    Phone = "000000000",
                    Password = "Password123"
                },
                Guid.NewGuid()));
    }

    // =====================================================
    // REFRESH TOKEN
    // =====================================================

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var (_, repo, tenantId) = await SeedUser();

        var refreshRepo = new FakeRefreshTokenRepository();

        var authService = new AuthService(
            repo,
            CreateJwtService(),
            refreshRepo,
            _passwordHasher);

        // Login first
        var loginResponse =
            await authService.LoginAsync(
                new LoginRequest
                {
                    Phone = "123456789",
                    Password = "Password123"
                },
                tenantId);

        // Act
        var refreshed =
            await authService.RefreshAsync(
                new RefreshTokenRequest
                {
                    RefreshToken = loginResponse.RefreshToken
                },
                tenantId);

        // Assert
        Assert.NotNull(refreshed);

        Assert.NotEmpty(refreshed.AccessToken);

        Assert.NotEmpty(refreshed.RefreshToken);

        Assert.NotEqual(
            loginResponse.RefreshToken,
            refreshed.RefreshToken);
    }

    // =====================================================
    // LOGOUT
    // =====================================================

    [Fact]
    public async Task Logout_ShouldRevokeRefreshToken()
    {
        // Arrange
        var (_, repo, tenantId) = await SeedUser();

        var refreshRepo = new FakeRefreshTokenRepository();

        var authService = new AuthService(
            repo,
            CreateJwtService(),
            refreshRepo,
            _passwordHasher);

        var login =
            await authService.LoginAsync(
                new LoginRequest
                {
                    Phone = "123456789",
                    Password = "Password123"
                },
                tenantId);

        // Act
        await authService.LogoutAsync(login.RefreshToken);

        // Assert
        var token =
            await refreshRepo.GetByTokenAsync(
                login.RefreshToken);

        Assert.NotNull(token);

        Assert.True(token!.IsRevoked);
    }
}