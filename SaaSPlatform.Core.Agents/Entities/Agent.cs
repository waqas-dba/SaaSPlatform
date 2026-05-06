using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Agents.Entities;

public class Agent : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    // commission model
    public decimal CommissionRate { get; set; } // e.g. 10%
}