// SaaSPlatform.Core/Tenant/Models/TenantRegistrationResponse.cs
namespace SaaSPlatform.Core.Tenant.Models;

public class TenantRegistrationResponse
{
    public Guid TenantId { get; set; }
    public string Message { get; set; } = default!;
}