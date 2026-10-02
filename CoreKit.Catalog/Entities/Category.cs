using CoreKit.SharedKernel.Common;

namespace CoreKit.Catalog.Entities;

public class Category : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public int Level { get; set; } = 1;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}