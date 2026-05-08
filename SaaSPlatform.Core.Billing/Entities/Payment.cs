// SaaSPlatform.Core/Billing/Entities/Payment.cs
using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Billing.Entities;

public class Payment : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid SubscriptionId { get; set; }

    // NEW – optional store association (null = tenant‑level payment)
    public Guid? StoreId { get; set; }

    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string Provider { get; set; } = "Cash";
    public DateTime? PaidAt { get; set; }
    public DateTime? StatusUpdatedAt { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
}