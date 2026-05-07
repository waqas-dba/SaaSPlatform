using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Billing.Entities;

public class Payment : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid SubscriptionId { get; set; }

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string Provider { get; set; } = default!;
    // Stripe / JazzCash / EasyPaisa

    public string? TransactionId { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? FailureReason { get; set; }
}