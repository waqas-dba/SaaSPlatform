// CoreKit.IAM | CoreKit.IAM/Extensions/ServiceCollectionExtensions.cs
using System.Text;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
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
        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new Exception("JWT configuration missing.");

        services.Configure<JwtSettings>(jwtSection);

        var iamOptions = new IamOptions();
        configureOptions?.Invoke(iamOptions);
        services.AddSingleton(iamOptions);

        services.AddHttpContextAccessor();

        services.AddDbContext<IamDbContext>(options =>
            options.UseNpgsql(connectionString));

        // FIX: IMemoryCache is required by PermissionCacheService.
        // AddMemoryCache is idempotent — safe to call multiple times.
        services.AddMemoryCache();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<CurrentUserService>();
        services.AddScoped<ICurrentUserService>(sp =>
            sp.GetRequiredService<CurrentUserService>());
        services.AddScoped<ICurrentUser>(sp =>
            sp.GetRequiredService<CurrentUserService>());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();

        // FIX: register cache service before PermissionService which depends on it
        services.AddSingleton<IPermissionCacheService, PermissionCacheService>();
        services.AddScoped<IPermissionService, PermissionService>();

        if (iamOptions.EnableUserDocuments)
            services.AddScoped<IUserDocumentService, UserDocumentService>();

        if (iamOptions.EnableUserIdentities)
            services.AddScoped<IUserIdentityService, UserIdentityService>();

        if (iamOptions.EnableRoleDocumentRequirements)
            services.AddScoped<IRoleDocumentRequirementService,
                RoleDocumentRequirementService>();

        // FIX: EncryptionService is registered here from configuration so
        // Program.cs files don't need to duplicate it.
        services.AddSingleton<IEncryptionService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var key = config["EncryptionKey"];
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("EncryptionKey is missing from configuration.");
            return new EncryptionService(key);
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}