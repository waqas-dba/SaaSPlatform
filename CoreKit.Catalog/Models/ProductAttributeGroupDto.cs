// CoreKit.Catalog/Models/AttributeTemplate/ProductAttributeGroupDto.cs
namespace CoreKit.Catalog.Models;

public class ProductAttributeGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string StoreTypeCode { get; set; } = default!;
    public int SortOrder { get; set; }
    public Guid? TenantId { get; set; }
    public List<ProductAttributeTemplateDto> Attributes { get; set; } = new();
}