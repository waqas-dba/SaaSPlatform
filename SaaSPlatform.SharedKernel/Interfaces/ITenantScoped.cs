namespace SaaSPlatform.Core.SharedKernel.Common;

public interface ITenantScoped
{
    Guid TenantId { get; set; }
}