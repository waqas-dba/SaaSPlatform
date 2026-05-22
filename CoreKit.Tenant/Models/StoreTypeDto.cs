namespace CoreKit.Tenant.Models;

public class StoreTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Category { get; set; } = default!;
}