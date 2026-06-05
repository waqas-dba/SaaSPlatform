// CoreKit.IAM | Interfaces/IAuditLogRepository.cs
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Models;

namespace CoreKit.IAM.Interfaces;

public interface IAuditLogRepository
{
    Task<PagedResult<AuditLog>> GetLogsAsync(
        string? entityName = null,
        string? entityId = null,
        PagedQuery? query = null,
        CancellationToken ct = default);
}