namespace SaaSPlatform.Tenant.Api;


public class CreateTenantUserRequest
{
    public string Name { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public string Password { get; set; } = default!;
}

public class UpdateTenantUserRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class AssignTenantRoleRequest
{
    public Guid RoleId { get; set; }
}
