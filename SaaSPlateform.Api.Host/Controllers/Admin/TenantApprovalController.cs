using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Api.Host.Authorization;
using SaaSPlatform.Api.Host.Controllers;

[ApiController]
[Route("api/admin")]
public class TenantApprovalController : BaseApiController
{
    private readonly TenantApprovalService _approvalService;

    public TenantApprovalController(TenantApprovalService approvalService)
        => _approvalService = approvalService;

    [TenantPermission("tenant.approve")]
    [HttpPost("tenants/{tenantId}/approve")]
    public async Task<IActionResult> ApproveTenant(Guid tenantId)
    {
        var (success, message) = await _approvalService.ApproveAsync(tenantId);

        if (!success)
            return Fail(message);

        return Success(new { }, message);
    }

    [TenantPermission("tenant.approve")]
    [HttpPost("tenants/{tenantId}/reject")]
    public async Task<IActionResult> RejectTenant(Guid tenantId)
    {
        var (success, message) = await _approvalService.RejectAsync(tenantId);

        if (!success)
            return Fail(message);

        return Success(new { }, message);
    }
}