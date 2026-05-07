using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SaaSPlatform.Infrastructure.Services.IAM;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(
        User user,
        Guid tenantId,
        IEnumerable<string> roles)
    {
        var expiresAt = DateTime.UtcNow
            .AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new("userId", user.Id.ToString()),
            new("tenantId", tenantId.ToString()),
            new(ClaimTypes.MobilePhone, user.Phone),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // roles → claims
        claims.AddRange(
            roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Secret));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return (jwt, expiresAt);
    }
}