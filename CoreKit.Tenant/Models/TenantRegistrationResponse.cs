namespace CoreKit.Tenant.Models;

public class TenantRegistrationResponse
{
    public Guid TenantId { get; set; }
    public string Message { get; set; } = default!;
}