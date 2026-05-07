using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Billing.Entities;

public class Payment : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid SubscriptionId { get; set; }

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // MVP = Cash only, but extensible later
    public string Provider { get; set; } = "Cash";

    public DateTime? PaidAt { get; set; }

    public DateTime? StatusUpdatedAt { get; set; }

    // future-safe optional fields
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
}