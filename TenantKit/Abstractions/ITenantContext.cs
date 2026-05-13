namespace TenantKit.Abstractions;

public interface ITenantContext
{
    Guid? TenantId { get; }
}