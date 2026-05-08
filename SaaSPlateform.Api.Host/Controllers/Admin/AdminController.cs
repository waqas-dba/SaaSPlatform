using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Api.Host.Authorization;
using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Application.Services;

namespace SaaSPlatform.Api.Host.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly TenantApprovalService _approvalService;

    public AdminController(TenantApprovalService approvalService) => _approvalService = approvalService;

    [TenantPermission("tenant.approve")]
    [HttpPost("tenants/{tenantId}/approve")]
    public async Task<IActionResult> ApproveTenant(Guid tenantId)
    {
        var (success, message) = await _approvalService.ApproveAsync(tenantId);
        if (!success) return BadRequest(ApiResponse<object>.FailResponse(message));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, message));
    }

    [TenantPermission("tenant.approve")]
    [HttpPost("tenants/{tenantId}/reject")]
    public async Task<IActionResult> RejectTenant(Guid tenantId)
    {
        var (success, message) = await _approvalService.RejectAsync(tenantId);
        if (!success) return BadRequest(ApiResponse<object>.FailResponse(message));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, message));
    }
}
