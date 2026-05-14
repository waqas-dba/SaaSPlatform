namespace CoreKit.Tenant.Entities;

public class TenantLegalInfo
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? BusinessLicenseNumber { get; set; }
    public string? TaxId { get; set; }
    public string? AdditionalJson { get; set; }
    public TenantEntity Tenant { get; set; } = default!;
}