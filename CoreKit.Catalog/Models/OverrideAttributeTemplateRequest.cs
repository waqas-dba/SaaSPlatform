// CoreKit.Catalog/Models/AttributeTemplate/OverrideAttributeTemplateRequest.cs
namespace CoreKit.Catalog.Models;

/// <summary>
/// Sent by a tenant admin to create a tenant-scoped override of a
/// platform attribute template. The platform row is never modified.
/// </summary>
public class OverrideAttributeTemplateRequest
{
    /// <summary>The platform template being overridden.</summary>
    public Guid PlatformTemplateId { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>Only the fields the tenant wants to change.</summary>
    public string? Name { get; set; }
    public string? OptionsJson { get; set; }
    public bool? IsRequired { get; set; }
    public bool? IsVisible { get; set; }
    public int? SortOrder { get; set; }
    public Guid? GroupId { get; set; }
}