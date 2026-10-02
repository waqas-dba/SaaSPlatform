namespace CoreKit.Catalog.Models;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int Level { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = new();
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = default!;
    public string? IconUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateCategoryRequest
{
    public string? Name { get; set; }
    public string? IconUrl { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}