// CoreKit.Catalog/Entities/Category.cs

using CoreKit.SharedKernel.Common;

namespace CoreKit.Catalog.Entities;

public class Category : AuditableEntity, ITenantScoped
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? IconUrl { get; set; }

    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public int Level { get; set; } = 1;
    public string? StoreTypeCode { get; set; }
    public Guid? StoreId { get; set; }
    public Guid TenantId { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();

    // Was ICollection<AttributeTemplate> — now correctly points to
    // ProductAttributeTemplate, removing the dead duplicate entity.
    public ICollection<ProductAttributeTemplate> AttributeTemplates { get; set; }
        = new List<ProductAttributeTemplate>();
}