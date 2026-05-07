using SaaSPlatform.Core.IAM.Entities;

namespace SaaSPlatform.Core.IAM.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(
        User user,
        Guid tenantId,
        IEnumerable<string> roles);
}