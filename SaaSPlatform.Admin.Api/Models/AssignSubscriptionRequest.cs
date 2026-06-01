namespace SaaSPlatform.Admin.Api.Models;

public class AssignSubscriptionRequest
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}