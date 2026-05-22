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
    private readonly ISubscriptionManagementService _subscriptionService;

    public SubscriptionsController(ISubscriptionManagementService subscriptionService)
        => _subscriptionService = subscriptionService;

    [HttpPost]
    [RequiresPermission(Permissions.Platform.ManageSystemSettings)]
    public async Task<IActionResult> Assign([FromBody] AssignSubscriptionRequest request)
    {
        await _subscriptionService.AssignSubscriptionAsync(
            request.TenantId, request.PlanId, request.StartDate, request.EndDate);
        return CreatedResponse("Subscription assigned successfully.");
    }

    [HttpGet("{tenantId}")]
    [RequiresPermission(Permissions.Platform.ViewAllTenants)]
    public async Task<IActionResult> GetByTenant(Guid tenantId)
    {
        var sub = await _subscriptionService.GetActiveSubscriptionForTenantAsync(tenantId);
        if (sub == null) return NotFound("No active subscription found.");
        return Ok(new { sub.PlanId, sub.Plan.Name, sub.StartDate, sub.EndDate, sub.Status });
    }
}

public class AssignSubscriptionRequest
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}