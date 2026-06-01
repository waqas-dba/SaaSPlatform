namespace CoreKit.Catalog.Models;

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; }
    public Guid? AddonGroupId { get; set; }
    public bool? IsActive { get; set; }
}