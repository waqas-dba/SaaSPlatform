using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Api.Host.Authorization;

namespace SaaSPlatform.Api.Host.Controllers.Admin;

[ApiController]
[Route("api/admin")]
public class TenantApprovalController : BaseApiController
{
    private readonly TenantApprovalService _service;

    public TenantApprovalController(TenantApprovalService service)
    {
        _service = service;
    }

    [TenantPermission("tenant.approve")]
    [HttpPost("tenants/{tenantId}/approve")]
    public async Task<IActionResult> Approve(Guid tenantId)
    {
        var (ok, msg) = await _service.ApproveAsync(tenantId);

        if (!ok) return Fail(msg);
        return Success(new { }, msg);
    }

    [TenantPermission("tenant.approve")]
    [HttpPost("tenants/{tenantId}/reject")]
    public async Task<IActionResult> Reject(Guid tenantId)
    {
        var (ok, msg) = await _service.RejectAsync(tenantId);

        if (!ok) return Fail(msg);
        return Success(new { }, msg);
    }
}