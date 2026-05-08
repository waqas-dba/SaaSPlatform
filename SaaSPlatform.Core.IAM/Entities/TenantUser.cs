using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class TenantUser : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }

    public bool IsOwner { get; set; }
    public bool IsActive { get; set; } = true;
}