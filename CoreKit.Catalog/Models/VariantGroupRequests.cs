// CoreKit.Catalog/Models/VariantGroupRequests.cs
namespace CoreKit.Catalog.Models;

public class CreateVariantGroupRequest
{
    public string Name { get; set; } = default!;
    public string StoreTypeCode { get; set; } = default!;

    /// <summary>
    /// Dimensions to include in the group at creation time.
    /// Can be empty and added later.
    /// </summary>
    public List<VariantGroupOptionRequest> Options { get; set; } = new();
}

public class UpdateVariantGroupRequest
{
    public string? Name { get; set; }
}

public class VariantGroupOptionRequest
{
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Subset of the template's OptionsJson values this merchant sells.
    /// Null = allow all values from the template.
    /// </summary>
    public List<string>? AllowedValues { get; set; }

    public int SortOrder { get; set; }
}