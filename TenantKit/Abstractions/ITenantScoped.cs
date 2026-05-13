namespace TenantKit.Abstractions;

public interface ITenantScoped
{
    Guid TenantId { get; set; }
}