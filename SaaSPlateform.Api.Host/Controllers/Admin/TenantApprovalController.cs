using AuthCoreKit.IAM.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantKit.Interfaces;

namespace SaaSPlatform.Api.Host.Controllers.Admin;

[ApiController]
[Route("api/admin/tenants")]
public class TenantApprovalController : BaseApiController
{
    private readonly ITenantService _tenantService;

    public TenantApprovalController(ITenantService tenantService) => _tenantService = tenantService;

    [HttpPost("{tenantId}/approve")]
    [RequiresPermission("tenant.approve")]
    public async Task<IActionResult> Approve(Guid tenantId)
    {
        await _tenantService.ApproveAsync(tenantId);
        return Success(new { }, "Tenant approved");
    }

    [HttpPost("{tenantId}/reject")]
    [RequiresPermission("tenant.reject")]
    public async Task<IActionResult> Reject(Guid tenantId)
    {
        await _tenantService.RejectAsync(tenantId);
        return Success(new { }, "Tenant rejected");
    }
}