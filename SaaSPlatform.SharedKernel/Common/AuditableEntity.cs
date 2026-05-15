namespace CoreKit.SharedKernel.Common;

// ✅ Now implements ISoftDelete so soft-delete global filter works for all entities.
public abstract class AuditableEntity : BaseEntity, ISoftDelete
{
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}