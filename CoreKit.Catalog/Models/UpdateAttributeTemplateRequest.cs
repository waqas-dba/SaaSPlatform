// CoreKit.Catalog/Models/AttributeTemplate/UpdateAttributeTemplateRequest.cs
namespace CoreKit.Catalog.Models;

public class UpdateAttributeTemplateRequest
{
    public string? Name { get; set; }
    public string? OptionsJson { get; set; }
    public bool? IsRequired { get; set; }
    public bool? IsVisible { get; set; }
    public int? SortOrder { get; set; }
    public Guid? GroupId { get; set; }
}