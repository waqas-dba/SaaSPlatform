using Microsoft.EntityFrameworkCore;
using AuthCoreKit.IAM.Entities;

namespace AuthCoreKit.IAM.Interfaces;

public interface IIamDbContext
{
    // Existing sets
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<PermissionModule> PermissionModules { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    // NEW – document‑related sets
    DbSet<UserDocument> UserDocuments { get; }
    DbSet<UserIdentity> UserIdentities { get; }
    DbSet<RoleDocumentRequirement> RoleDocumentRequirements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}