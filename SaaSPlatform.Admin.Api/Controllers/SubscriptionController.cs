using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Subscription.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/subscription")]
[Authorize]
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
    public async Task<IActionResult> GetAllPlans()
    {
        var plans = await _planService.GetAllAsync();
        return Ok(plans);
    }

    [HttpPost("assign")]
    [RequiresPermission(Permissions.Platform.ManageBilling)]
    public async Task<IActionResult> AssignSubscription([FromBody] AssignSubscriptionRequest request)
    {
        await _subService.AssignSubscriptionAsync(
            request.TenantId,
            request.PlanId,
            startDate: request.StartDate,
            endDate: request.EndDate);

        var active = await _subService.GetActiveSubscriptionForTenantAsync(request.TenantId);
        return Ok(active);
    }

    [HttpGet("tenant/{tenantId}/active")]
    [RequiresPermission(Permissions.Platform.ManageBilling)]
    public async Task<IActionResult> GetActiveSubscription(Guid tenantId)
    {
        var sub = await _subService.GetActiveSubscriptionForTenantAsync(tenantId);
        if (sub is null)
            return NotFound(new
            {
                success = false,
                errorCode = "NOT_FOUND",
                message = "No active subscription found."
            });

        return Ok(sub);
    }
}