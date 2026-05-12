using Microsoft.EntityFrameworkCore;
using SaaSPlatform.SharedKernel.Common;
using AuthCoreKit.IAM.Interfaces;   // <-- already present

namespace SaaSPlatform.Infrastructure.Persistence;

/// <summary>
/// Handles automatic audit tracking.
/// </summary>
public abstract class AuditableDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;   // <-- CHANGED from CurrentUserService?

    protected AuditableDbContext(
        DbContextOptions options,
        ICurrentUserService? currentUser = null)           // <-- CHANGED from CurrentUserService?
        : base(options)
    {
        _currentUser = currentUser;
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var entries =
            ChangeTracker
                .Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUser?.UserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentUser?.UserId;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}