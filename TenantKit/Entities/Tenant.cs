using TenantKit.Abstractions;

namespace TenantKit.Domain.Entities;

public class Tenant : AuditableEntity
{
    public string Name { get; set; } = default!;

    public string Slug { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    public string? MetadataJson { get; set; }
}