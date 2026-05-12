namespace AuthCoreKit.IAM.Entities;

public class PermissionModule : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public ICollection<Permission>? Permissions { get; set; }
}