// CoreKit.Catalog/Models/AddonGroupRequest.cs
namespace CoreKit.Catalog.Models;

public class CreateAddonGroupRequest
{
    public string Name { get; set; } = default!;

    /// <summary>
    /// Addons to create together with the group.
    /// Can be empty — addons can be added later.
    /// </summary>
    public List<CreateAddonItem> Addons { get; set; } = new();
}

public class UpdateAddonGroupRequest
{
    public string? Name { get; set; }
}

public class AddAddonToGroupRequest
{
    public string Name { get; set; } = default!;
    public decimal AdditionalPrice { get; set; }
}