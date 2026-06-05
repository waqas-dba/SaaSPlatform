// CoreKit.SharedKernel | Common/AuditLog.cs
namespace CoreKit.SharedKernel.Common;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string Action { get; set; } = default!; // "Created", "Updated", "Deleted"
    public string? ChangedBy { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}