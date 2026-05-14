using CoreKit.IAM.Interfaces;        // internal repositories
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
using CoreKit.IAM.Repositories;
using CoreKit.IAM.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CoreKit.IAM.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreKitIAM(
        this IServiceCollection services,
        string connectionString,
        IConfigurationSection jwtSection,
        Action<IamOptions>? configureOptions = null)
    {
        // JWT settings
        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT configuration missing.");
        services.Configure<JwtSettings>(jwtSection);

        // Database
        services.AddDbContext<IamDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Options
        var options = new IamOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // Internal repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Core IAM internal services (for module’s own use)
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ===== Contract‑facing services (what the host sees) =====
        services.AddScoped<IAuthService, AuthService>();                   // CoreKit.Contracts.Interfaces.IAuthService
        services.AddScoped<IUserManagementService, UserManagementService>(); // same as above but kept for direct host usage
        services.AddScoped<IPermissionService, PermissionService>();       // already registered, but we keep the mapping for contracts
        services.AddScoped<IRoleManagementService, RoleManagementService>();

        // Encryption service – host should override with a real implementation.
        if (!services.Any(s => s.ServiceType == typeof(IEncryptionService)))
        {
            services.AddSingleton<IEncryptionService>(new DefaultEncryptionService());
        }

        if (options.EnableUserDocuments)
        {
            services.AddScoped<IUserDocumentService, UserDocumentService>();
            if (options.EnableUserIdentities)
                services.AddScoped<IUserIdentityService, UserIdentityService>();
            if (options.EnableRoleDocumentRequirements)
                services.AddScoped<IRoleDocumentRequirementService, RoleDocumentRequirementService>();
        }

        // JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        services.AddAuthorization();

        return services;
    }
}

// Default encryption service (throws if used)
internal class DefaultEncryptionService : IEncryptionService
{
    public string Encrypt(string plainText) => throw new NotSupportedException("Encryption not configured.");
    public string Decrypt(string cipherText) => throw new NotSupportedException("Encryption not configured.");
    public byte[] Encrypt(byte[] plainBytes) => throw new NotSupportedException("Encryption not configured.");
    public byte[] Decrypt(byte[] encryptedBytes) => throw new NotSupportedException("Encryption not configured.");
}