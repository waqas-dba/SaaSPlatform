using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Tenant.Entities;

public class Tenant : AuditableEntity
{
    public string Name { get; set; } = default!;

    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    // Branding
    public string? LogoUrl { get; set; }

    // SEO / SaaS routing (VERY IMPORTANT)
    public string? Slug { get; set; }   // example: "pizza-hub"

    // 🔥 AGENT WHO BROUGHT THIS TENANT
    public Guid? AgentId { get; set; }

    // 🔥 CURRENT ACTIVE SUBSCRIPTION (snapshot reference)
    public Guid? ActiveSubscriptionId { get; set; }

    // onboarding tracking
    public DateTime? OnboardedAt { get; set; }

    // Navigation
    public ICollection<TenantDomain>? Domains { get; set; }
}