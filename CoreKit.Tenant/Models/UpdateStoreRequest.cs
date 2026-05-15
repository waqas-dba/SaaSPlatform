namespace CoreKit.Tenant.Models;

public class UpdateStoreRequest
{
    public string? Name { get; set; }
    public string? Type { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public string? MetadataJson { get; set; }
}