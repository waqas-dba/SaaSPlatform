namespace CoreKit.IAM.Entities;

/// <summary>
/// Defines a document type that is required for a given role.
/// </summary>
public class RoleDocumentRequirement
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string DocumentType { get; set; } = default!;   // e.g. "cnic_front", "driving_license_front"

    public Role Role { get; set; } = default!;
}