// SaaSPlatform.Admin.Api/Controllers/SubscriptionsController.cs
// SubscriptionController.cs (singular) has been deleted.
// All subscription admin routes live here under /api/admin/subscriptions.

using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Subscription.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/subscriptions")]
[Authorize]
public class SubscriptionsController : ApiControllerBase
{
    private readonly IPlanService _planService;
    private readonly ISubscriptionManagementService _subscriptionService;

    public SubscriptionsController(
        IPlanService planService,
        ISubscriptionManagementService subscriptionService)
    {
        _planService = planService;
        _subscriptionService = subscriptionService;
    }

    [HttpGet("plans")]
    [RequiresPermission(Permissions.Platform.ManageBilling)]
    public async Task<IActionResult> GetAllPlans()
    {
        var plans = await _planService.GetAllAsync();
        return OkResponse(plans);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Platform.ManageBilling)]
    public async Task<IActionResult> Assign(
        [FromBody] AssignSubscriptionRequest request)
    {
        await _subscriptionService.AssignSubscriptionAsync(
            request.TenantId,
            request.PlanId,
            request.StartDate,
            request.EndDate);

        return CreatedResponse("Subscription assigned successfully.");
    }

    [HttpGet("{tenantId}")]
    [RequiresPermission(Permissions.Platform.ManageBilling)]
    public async Task<IActionResult> GetByTenant(Guid tenantId)
    {
        var sub = await _subscriptionService
            .GetActiveSubscriptionForTenantAsync(tenantId);

        if (sub is null)
            return NotFound(new
            {
                success = false,
                errorCode = "NOT_FOUND",
                message = "No active subscription found."
            });

        return OkResponse(new
        {
            sub.PlanId,
            sub.Plan.Name,
            sub.StartDate,
            sub.EndDate,
            sub.Status
        });
    }
}

public class AssignSubscriptionRequest
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}