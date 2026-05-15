using System.Text;

using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
using CoreKit.IAM.Persistence.Seeders;
using CoreKit.IAM.Repositories;
using CoreKit.IAM.Services;

using CoreKit.SharedKernel.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CoreKit.IAM.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreKitIAM(
        this IServiceCollection services,
        string connectionString,
        IConfigurationSection jwtSection,
        Action<IamOptions>? configureOptions = null)
    {
        // =========================
        // JWT SETTINGS
        // =========================
        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new Exception("JWT configuration missing.");

        services.Configure<JwtSettings>(jwtSection);

        // =========================
        // IAM OPTIONS
        // =========================
        var iamOptions = new IamOptions();

        configureOptions?.Invoke(iamOptions);

        services.AddSingleton(iamOptions);

        // =========================
        // HTTP CONTEXT
        // =========================
        services.AddHttpContextAccessor();

        // =========================
        // DB CONTEXT
        // =========================
        services.AddDbContext<IamDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // =========================
        // REPOSITORIES
        // =========================
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IRoleRepository, RoleRepository>();

        services.AddScoped<IPermissionRepository, PermissionRepository>();

        services.AddScoped<IRefreshTokenRepository,
            RefreshTokenRepository>();

        // =========================
        // CURRENT USER
        // =========================
        services.AddScoped<CurrentUserService>();

        services.AddScoped<ICurrentUserService>(sp =>
            sp.GetRequiredService<CurrentUserService>());

        services.AddScoped<ICurrentUser>(sp =>
            sp.GetRequiredService<CurrentUserService>());

        // =========================
        // CORE SERVICES
        // =========================
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddScoped<IRefreshTokenService,
            RefreshTokenService>();

        services.AddScoped<IUserManagementService,
            UserManagementService>();

        services.AddScoped<IRoleManagementService,
            RoleManagementService>();

        services.AddScoped<IPermissionService,
            PermissionService>();

        // =========================
        // OPTIONAL FEATURES
        // =========================
        if (iamOptions.EnableUserDocuments)
        {
            services.AddScoped<IUserDocumentService,
                UserDocumentService>();
        }

        if (iamOptions.EnableUserIdentities)
        {
            services.AddScoped<IUserIdentityService,
                UserIdentityService>();
        }

        if (iamOptions.EnableRoleDocumentRequirements)
        {
            services.AddScoped<
                IRoleDocumentRequirementService,
                RoleDocumentRequirementService>();
        }

        // =========================
        // ENCRYPTION
        // =========================
        services.AddSingleton<IEncryptionService>(sp =>
        {
            var configuration =
                sp.GetRequiredService<IConfiguration>();

            var key = configuration["EncryptionKey"];

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new Exception(
                    "EncryptionKey missing.");
            }

            return new EncryptionService(key);
        });

        // =========================
        // SEEDER
        // =========================
        services.AddScoped<IamSeeder>();

        // =========================
        // JWT AUTHENTICATION
        // =========================
        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings.Issuer,

                        ValidAudience = jwtSettings.Audience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtSettings.Secret)
                            ),

                        ClockSkew = TimeSpan.Zero
                    };
            });

        services.AddAuthorization();

        return services;
    }
}