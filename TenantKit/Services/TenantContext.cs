using TenantKit.Abstractions;

namespace TenantKit.Services;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; internal set; }
}