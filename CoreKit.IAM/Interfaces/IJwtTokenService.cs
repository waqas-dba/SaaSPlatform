// CoreKit.IAM/Interfaces/IJwtTokenService.cs
using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Interfaces;

public interface IJwtTokenService
{
    // FIX: named tuple members so callers can use .Token / .ExpiresAt / .JwtId
    (string Token, DateTime ExpiresAt, string JwtId) GenerateAccessToken(
        User user,
        Guid? tenantId,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions = null,
        IEnumerable<Guid>? storeIds = null);
}