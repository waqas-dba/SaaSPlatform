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
            // Automatically bind IIamDbContext to the host's DbContext
            services.AddScoped<IIamDbContext>(sp => sp.GetRequiredService<TDbContext>());

            var jwtSettings = jwtSection.Get<JwtSettings>()
                              ?? throw new InvalidOperationException("JWT configuration section is missing.");

            services.Configure<JwtSettings>(jwtSection);

            var options = new IamOptions();
            configureOptions?.Invoke(options);
            services.AddSingleton(options);

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Services
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRoleManagementService, RoleManagementService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

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