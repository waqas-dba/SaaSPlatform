namespace CoreKit.IAM.Models;

public class UserListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Email { get; set; }
    public string Phone { get; set; } = default!;
    public bool IsActive { get; set; }
    public Guid? TenantId { get; set; }
}