namespace CoreKit.Tenant.Models;

public class TenantRegistrationRequest
{
    public string Name { get; set; } = default!;
    public string? MetadataJson { get; set; }
}