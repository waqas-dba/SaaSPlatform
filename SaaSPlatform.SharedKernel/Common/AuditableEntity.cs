// CoreKit.SharedKernel | CoreKit.SharedKernel/Common/AuditableEntity.cs
namespace CoreKit.SharedKernel.Common;

public abstract class AuditableEntity : BaseEntity, ISoftDelete
{
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }

    // FIX: optimistic concurrency — EF will throw DbUpdateConcurrencyException
    // if two requests try to save the same row simultaneously.
    // PostgreSQL uses xmin (system column); we map it via ValueGeneratedOnAddOrUpdate.
    public uint RowVersion { get; set; }
}