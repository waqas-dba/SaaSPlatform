using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;

namespace CoreKit.IAM.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(
        User user,
        Guid? tenantId,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions = null,
        IEnumerable<Guid>? storeIds = null)       // ← new optional parameter
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);
        var jwtId = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(ClaimConstants.UserId, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(ClaimConstants.Name, user.Name)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimConstants.Email, user.Email));
        }

        if (tenantId.HasValue)
        {
            claims.Add(new Claim(ClaimConstants.TenantId, tenantId.Value.ToString()));
        }

        foreach (var role in roles.Distinct())
        {
            claims.Add(new Claim(ClaimConstants.Role, role));
        }

        if (permissions != null)
        {
            foreach (var permission in permissions.Distinct())
            {
                claims.Add(new Claim(ClaimConstants.Permission, permission));
            }
        }

        // ---------- Store claims (fast path for StoreScope resolution) ----------
        if (storeIds != null)
        {
            foreach (var storeId in storeIds.Distinct())
            {
                claims.Add(new Claim(ClaimConstants.StoreId, storeId.ToString()));
            }
        }

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