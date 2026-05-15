namespace CoreKit.IAM.Models;

public class AssignRoleWithTenantRequest
{
    public Guid RoleId { get; set; }
    public Guid? TenantId { get; set; }   // the tenant where the user/role belong
}