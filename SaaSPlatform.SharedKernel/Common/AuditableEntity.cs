namespace CoreKit.SharedKernel.Common;

public abstract class AuditableEntity : BaseEntity, ISoftDelete
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }

    // Concurrency token backed by PostgreSQL xmin
    public uint RowVersion { get; set; }
}