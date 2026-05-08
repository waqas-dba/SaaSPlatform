namespace SaaSPlatform.SharedKernel.Common;

/// <summary>
/// Base auditable entity.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// Created date UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Created by user id.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Updated date UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Updated by user id.
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }
}