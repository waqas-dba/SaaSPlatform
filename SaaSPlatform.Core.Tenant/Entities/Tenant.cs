using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.SharedKernel.Common;
using SaaSPlatform.Core.Tenant.Enums;

namespace SaaSPlatform.Core.Tenant.Entities;

public class Tenant : AuditableEntity
{
    public string Name { get; set; } = default!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    // GEO LOCATION
    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    public RegistrationStatus RegistrationStatus { get; set; } = RegistrationStatus.Pending;

    // Branding
    public string? LogoUrl { get; set; }

    // SEO / ROUTING
    public string? Slug { get; set; }

    // AGENT
    public Guid? AgentId { get; set; }

    // SUBSCRIPTION
    public Guid? ActiveSubscriptionId { get; set; }

    public DateTime? OnboardedAt { get; set; }

    public ICollection<TenantDomain>? Domains { get; set; }

    public ICollection<Subscription>? Subscriptions { get; set; }
}