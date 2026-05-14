namespace TenantKit.Entities;

public class TenantLegalInfo
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? BusinessLicenseNumber { get; set; }
    public string? TaxId { get; set; }
    public string? AdditionalJson { get; set; }
    public Tenant Tenant { get; set; } = default!;
}