using Asp.Versioning;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Subscription.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin/subscription")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class SubscriptionController : ApiControllerBase
{
    private readonly IPlanService _planService;
    private readonly ISubscriptionManagementService _subService;

    public SubscriptionController(
        IPlanService planService,
        ISubscriptionManagementService subService)
    {
        _planService = planService;
        _subService = subService;
    }

    [HttpGet("plans")]
    [RequiresPermission(Permissions.Platform.ManageBilling)]
    public async Task<IActionResult> GetAllPlans(CancellationToken ct)
    {
        var plans = await _planService.GetAllAsync();
        return OkResponse(plans);
    }

    [HttpPost("assign")]
    [RequiresPermission(Permissions.Platform.ManageBilling)]
    public async Task<IActionResult> AssignSubscription([FromBody] AssignSubscriptionRequest request, CancellationToken ct)
    {
        await _subService.AssignSubscriptionAsync(
            request.TenantId,
            request.PlanId,
            startDate: request.StartDate,
            endDate: request.EndDate);
        var active = await _subService.GetActiveSubscriptionForTenantAsync(request.TenantId);
        return OkResponse(active);
    }

    [HttpGet("tenant/{tenantId}/active")]
    [RequiresPermission(Permissions.Platform.ManageBilling)]
    public async Task<IActionResult> GetActiveSubscription(Guid tenantId, CancellationToken ct)
    {
        var sub = await _subService.GetActiveSubscriptionForTenantAsync(tenantId);
        if (sub is null)
            return NotFound(new { success = false, errorCode = "NOT_FOUND", message = "No active subscription found." });
        return OkResponse(sub);
    }
}

public class AssignSubscriptionRequest
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}