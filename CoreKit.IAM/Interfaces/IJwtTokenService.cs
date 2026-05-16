// CoreKit.IAM/Interfaces/IJwtTokenService.cs
using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Interfaces;

public interface IJwtTokenService
{
    // FIX: Return jwtId as the third element so callers can bind the
    // refresh token to the actual access token's jti claim.
    (string Token, DateTime ExpiresAt, string JwtId) GenerateAccessToken(
        User user,
        Guid? tenantId,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions = null,
        IEnumerable<Guid>? storeIds = null);
}