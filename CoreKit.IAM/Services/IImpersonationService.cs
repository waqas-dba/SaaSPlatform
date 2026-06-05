// CoreKit.IAM/Services/ImpersonationService.cs
using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;

namespace CoreKit.IAM.Services;

public interface IImpersonationService
{
    Task<(string Token, DateTime ExpiresAt)> CreateImpersonationTokenAsync(
        Guid platformUserId,
        Guid targetTenantId,
        string reason,
        CancellationToken ct = default);
}
