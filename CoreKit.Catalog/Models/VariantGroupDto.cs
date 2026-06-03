// CoreKit.Catalog/Models/VariantGroupDto.cs
namespace CoreKit.Catalog.Models;

public class VariantGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string StoreTypeCode { get; set; } = default!;
    public Guid TenantId { get; set; }
    public List<VariantGroupOptionDto> Options { get; set; } = new();
}

public class VariantGroupOptionDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = default!;

    /// <summary>
    /// Null means all values from the template are available.
    /// </summary>
    public List<string>? AllowedValues { get; set; }
    public int SortOrder { get; set; }
}