using TenantKit.Abstractions;

namespace TenantKit.Infrastructure;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; internal set; }
}