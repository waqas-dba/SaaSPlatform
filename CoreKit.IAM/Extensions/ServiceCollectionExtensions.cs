// CoreKit.IAM/Extensions/ServiceCollectionExtensions.cs
using System.Text;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
using CoreKit.IAM.Repositories;
using CoreKit.IAM.Services;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
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
            ?? throw new InvalidOperationException(
                "JWT configuration is missing. Ensure appsettings.json contains a 'Jwt' section " +
                "with Secret, Issuer, Audience, AccessTokenMinutes, and RefreshTokenDays.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
            throw new InvalidOperationException("Jwt:Secret is missing from configuration.");
        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            throw new InvalidOperationException("Jwt:Issuer is missing from configuration.");
        if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
            throw new InvalidOperationException("Jwt:Audience is missing from configuration.");

        services.Configure<JwtSettings>(jwtSection);

        var iamOptions = new IamOptions();
        configureOptions?.Invoke(iamOptions);
        services.AddSingleton(iamOptions);

        services.AddHttpContextAccessor();

        services.AddDbContext<IamDbContext>(options =>
            options.UseNpgsql(connectionString));

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
        services.AddSingleton<IPermissionCacheService, PermissionCacheService>();
        services.AddScoped<IPermissionService, PermissionService>();

        if (iamOptions.EnableUserDocuments)
            services.AddScoped<IUserDocumentService, UserDocumentService>();

        if (iamOptions.EnableUserIdentities)
            services.AddScoped<IUserIdentityService, UserIdentityService>();

        if (iamOptions.EnableRoleDocumentRequirements)
            services.AddScoped<IRoleDocumentRequirementService, RoleDocumentRequirementService>();

        services.AddSingleton<IEncryptionService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var key = config["EncryptionKey"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException(
                    "EncryptionKey is missing from configuration. " +
                    "Add a 32-byte base64-encoded key to appsettings.json: " +
                    "\"EncryptionKey\": \"<base64-32-bytes>\"");
            return new EncryptionService(key);
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
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

                o.Events = new JwtBearerEvents
                {
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        if (ctx.Response.HasStarted) return;

                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.WriteAsync(
                            "{\"success\":false," +
                            "\"errorCode\":\"UNAUTHORIZED\"," +
                            "\"message\":\"You are not logged in or your session has expired. " +
                            "Please log in and try again.\"}");
                    },

                    OnForbidden = async ctx =>
                    {
                        if (ctx.Response.HasStarted) return;

                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.WriteAsync(
                            "{\"success\":false," +
                            "\"errorCode\":\"FORBIDDEN\"," +
                            "\"message\":\"You do not have permission to perform this action. " +
                            "Contact your administrator if you believe this is incorrect.\"}");
                    },

                    OnAuthenticationFailed = async ctx =>
                    {
                        if (ctx.Response.HasStarted) return;

                        var (code, msg) = ctx.Exception is SecurityTokenExpiredException
                            ? ("TOKEN_EXPIRED",
                               "Your session has expired. Please log in again to continue.")
                            : ("TOKEN_INVALID",
                               "Your authentication token is invalid. Please log in again.");

                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.WriteAsync(
                            $"{{\"success\":false,\"errorCode\":\"{code}\"," +
                            $"\"message\":\"{msg}\"}}");
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}