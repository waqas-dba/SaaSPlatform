using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(
        User user,
        Guid? tenantId,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions = null,
        IEnumerable<Guid>? storeIds = null);   // ← add this optional parameter
}