// ProductAttributeValueDto.cs
namespace CoreKit.Catalog.Models;

public class ProductAttributeValueDto
{
    public Guid Id { get; set; }
    public string TemplateName { get; set; } = default!;
    public string Value { get; set; } = default!;
}