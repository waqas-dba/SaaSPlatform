using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Agents.Entities;

public class AgentCommission : AuditableEntity
{
    public Guid AgentId { get; set; }
    public Guid TenantId { get; set; }

    public decimal Amount { get; set; }
    public string Type { get; set; } = default!; // Signup / Subscription / Order

    public bool IsPaid { get; set; } = false;

    public DateTime? PaidAt { get; set; }
}