using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthCoreKit.IAM.Constants;
using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;

namespace AuthCoreKit.IAM.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options) => _settings = options.Value;

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, Guid? tenantId,
        IEnumerable<string> roles, IEnumerable<string>? permissions = null)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(ClaimConstants.UserId, user.Id.ToString()),
            new(ClaimConstants.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (tenantId.HasValue)
            claims.Add(new Claim(ClaimConstants.TenantId, tenantId.Value.ToString()));

        foreach (var role in roles.Distinct())
            claims.Add(new Claim(ClaimConstants.Role, role));

        if (permissions != null)
            foreach (var perm in permissions.Distinct())
                claims.Add(new Claim(ClaimConstants.Permission, perm));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}