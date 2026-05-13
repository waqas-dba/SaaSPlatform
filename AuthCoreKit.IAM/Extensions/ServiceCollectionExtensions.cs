using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Repositories;
using AuthCoreKit.IAM.Services;
using AuthCoreKit.IAM.Models;
using Microsoft.Extensions.Configuration;

namespace AuthCoreKit.IAM.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthCoreKit<TDbContext>(
            this IServiceCollection services,
            IConfigurationSection jwtSection,
            Action<IamOptions>? configureOptions = null)
            where TDbContext : class, IIamDbContext
        {
            // 1. Bind IIamDbContext to the host's DbContext
            services.AddScoped<IIamDbContext>(sp => sp.GetRequiredService<TDbContext>());

            // 2. Load JWT settings & IamOptions
            var jwtSettings = jwtSection.Get<JwtSettings>()
                              ?? throw new InvalidOperationException("JWT configuration section is missing.");

            services.Configure<JwtSettings>(jwtSection);

            var options = new IamOptions();
            configureOptions?.Invoke(options);
            services.AddSingleton(options);

            // 3. Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // 4. Core services
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRoleManagementService, RoleManagementService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // 5. Optional document‑related services
            if (options.EnableUserDocuments)
            {
                services.AddScoped<IUserDocumentService, UserDocumentService>();

                if (options.EnableUserIdentities)
                {
                    services.AddScoped<IUserIdentityService, UserIdentityService>();
                }

                if (options.EnableRoleDocumentRequirements)
                {
                    services.AddScoped<IRoleDocumentRequirementService, RoleDocumentRequirementService>();
                }
            }

            // 6. JWT Authentication & Authorization
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
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}