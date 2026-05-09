using Microsoft.EntityFrameworkCore;
using SaaSPlatform.SharedKernel.Common;
using SaaSPlatform.Infrastructure.Services.IAM;
using SaaSPlatform.Infrastructure.Services;

namespace SaaSPlatform.Infrastructure.Persistence;

/// <summary>
/// Handles automatic audit tracking.
/// </summary>
public abstract class AuditableDbContext : DbContext
{
    private readonly CurrentUserService? _currentUser;

    protected AuditableDbContext(
        DbContextOptions options,
        CurrentUserService? currentUser = null)
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

                    entry.Entity.CreatedBy =
                        _currentUser?.UserId;

                    break;

                case EntityState.Modified:

                    entry.Entity.UpdatedAt = DateTime.UtcNow;

                    entry.Entity.UpdatedBy =
                        _currentUser?.UserId;

                    break;
            }
        }

        return await base.SaveChangesAsync(
            cancellationToken);
    }
}