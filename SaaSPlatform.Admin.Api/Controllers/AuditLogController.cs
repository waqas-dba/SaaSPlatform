// SaaSPlatform.Admin.Api | Controllers/AuditLogController.cs
using Asp.Versioning;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController, Route("api/v{version:apiVersion}/admin/audit-logs"), Authorize, ApiVersion("1.0")]
public class AuditLogController : ApiControllerBase
{
    private readonly IAuditLogRepository _auditLogRepo;

    public AuditLogController(IAuditLogRepository auditLogRepo) => _auditLogRepo = auditLogRepo;

    [HttpGet]
    [RequiresPermission(Permissions.System.AuditLogs)]
    public async Task<IActionResult> Get(
        [FromQuery] string? entityName,
        [FromQuery] string? entityId,
        [FromQuery] PagedQuery query,
        CancellationToken ct)
    {
        var result = await _auditLogRepo.GetLogsAsync(entityName, entityId, query, ct);
        return OkResponse(result);
    }
}