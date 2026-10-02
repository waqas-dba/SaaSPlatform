namespace CoreKit.Catalog.Entities;

public class Addon
{
    public Guid Id { get; set; }
    public Guid AddonGroupId { get; set; }
    public AddonGroup AddonGroup { get; set; } = default!;

    public string Name { get; set; } = default!;
    public decimal AdditionalPrice { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}