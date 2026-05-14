namespace CoreKit.Contracts.Models;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? MetadataJson { get; set; }
}