using AuthCoreKit.IAM.Entities;

namespace AuthCoreKit.IAM.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, Guid? tenantId,
        IEnumerable<string> roles, IEnumerable<string>? permissions = null);
}