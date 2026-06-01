using CoreKit.SharedKernel.Common;

namespace CoreKit.Catalog.Entities;

public class AttributeTemplate : AuditableEntity, ITenantScoped
{
    public string Name { get; set; } = default!;
    public AttributeFieldType FieldType { get; set; }
    public string? OptionsJson { get; set; }
    public bool IsRequired { get; set; }

    public string? StoreTypeCode { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid TenantId { get; set; }

    public ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();
}

public enum AttributeFieldType
{
    Text = 1,
    Number = 2,
    Boolean = 3,
    Select = 4
}