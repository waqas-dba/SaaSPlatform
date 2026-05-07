using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Email { get; set; } = default!;
    public string Phone { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    public ICollection<UserRole>? Roles { get; set; }
}