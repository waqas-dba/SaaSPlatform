namespace SaaSPlatform.Admin.Api.Models;

public class CreateRoleRequest
{
    public string Name { get; set; } = default!;
    public Guid? TenantId { get; set; }
    public string? Description { get; set; }
}