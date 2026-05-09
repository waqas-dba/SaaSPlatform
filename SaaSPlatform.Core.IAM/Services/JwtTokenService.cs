using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaaSPlatform.Core.IAM.Constants;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Core.Tenant.Models;

namespace SaaSPlatform.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(
        IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(
        User user,
        Guid tenantId,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions = null)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(
            _jwtSettings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(
                ClaimConstants.UserId,
                user.Id.ToString()),

            new(
                ClaimConstants.TenantId,
                tenantId.ToString()),

            new(
                ClaimConstants.Email,
                user.Email),

            new(
                ClaimConstants.Subject,
                user.Id.ToString()),

            new(
                ClaimConstants.JwtId,
                Guid.NewGuid().ToString())
        };

        // Roles
        foreach (var role in roles.Distinct())
        {
            claims.Add(new Claim(
                ClaimConstants.Role,
                role));
        }

        // Permissions
        if (permissions != null)
        {
            foreach (var permission in permissions.Distinct())
            {
                claims.Add(new Claim(
                    ClaimConstants.Permission,
                    permission));
            }
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return (tokenValue, expiresAt);
    }
}