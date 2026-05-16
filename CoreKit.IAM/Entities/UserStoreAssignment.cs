namespace CoreKit.IAM.Entities;

public class UserStoreAssignment
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public Guid StoreId { get; set; }
    public Guid TenantId { get; set; }
}