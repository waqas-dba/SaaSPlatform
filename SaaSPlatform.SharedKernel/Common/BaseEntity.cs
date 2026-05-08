namespace SaaSPlatform.SharedKernel.Common;

/// <summary>
/// Base entity for all entities.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public Guid Id { get; set; }
}