namespace CoreKit.Catalog.Models;

public class AddonDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal AdditionalPrice { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class AddonGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int MinSelect { get; set; }
    public int MaxSelect { get; set; }
    public int SortOrder { get; set; }
    public List<AddonDto> Addons { get; set; } = new();
}

public class CreateAddonItem
{
    public string Name { get; set; } = default!;
    public decimal AdditionalPrice { get; set; }
    public int SortOrder { get; set; }
}

public class CreateAddonGroupRequest
{
    public string Name { get; set; } = default!;

    /// <summary>0 = optional group.</summary>
    public int MinSelect { get; set; }

    public int MaxSelect { get; set; } = 1;
    public int SortOrder { get; set; }

    /// <summary>Options created together with the group. May be empty.</summary>
    public List<CreateAddonItem> Addons { get; set; } = new();
}

public class UpdateAddonGroupRequest
{
    public string? Name { get; set; }
    public int? MinSelect { get; set; }
    public int? MaxSelect { get; set; }
    public int? SortOrder { get; set; }
}

public class AddAddonToGroupRequest
{
    public string Name { get; set; } = default!;
    public decimal AdditionalPrice { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateAddonRequest
{
    public string? Name { get; set; }
    public decimal? AdditionalPrice { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}