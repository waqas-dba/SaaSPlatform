// SaaSPlatform.Api.Host/Controllers/Admin/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Api.Host.Authorization;
using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.Tenant.Enums;
using SaaSPlatform.Infrastructure.Persistence;

namespace SaaSPlatform.Api.Host.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly SaaSPlatformDbContext _db;

    public AdminController(SaaSPlatformDbContext db) => _db = db;

    [TenantPermission("tenant.approve")]
    [HttpPost("tenants/{tenantId}/approve")]
    public async Task<IActionResult> ApproveTenant(Guid tenantId)
    {
        var tenant = await _db.Tenants
            .Include(t => t.Subscriptions)
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant is null) return NotFound();
        if (tenant.RegistrationStatus == RegistrationStatus.Approved)
            return BadRequest("Tenant is already approved.");

        tenant.IsActive = true;
        tenant.RegistrationStatus = RegistrationStatus.Approved;

        var subscription = tenant.Subscriptions?.FirstOrDefault();
        if (subscription != null && subscription.Status == SubscriptionStatus.Trialing)
        {
            subscription.Status = SubscriptionStatus.Active;
            subscription.TrialEndsAt = DateTime.UtcNow.AddDays(14);
            subscription.NextBillingDate = DateTime.UtcNow.AddDays(14);
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Tenant approved successfully." });
    }

    [TenantPermission("tenant.approve")]
    [HttpPost("tenants/{tenantId}/reject")]
    public async Task<IActionResult> RejectTenant(Guid tenantId)
    {
        var tenant = await _db.Tenants.FindAsync(tenantId);
        if (tenant is null) return NotFound();

        tenant.RegistrationStatus = RegistrationStatus.Rejected;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Tenant rejected." });
    }
}