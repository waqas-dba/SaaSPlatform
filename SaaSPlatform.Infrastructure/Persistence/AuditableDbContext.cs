using Microsoft.EntityFrameworkCore;
using SaaSPlatform.SharedKernel.Common;
using AuthCoreKit.IAM.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence;

public abstract class AuditableDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;

    protected AuditableDbContext(DbContextOptions options, ICurrentUserService? currentUser = null)
        : base(options) => _currentUser = currentUser;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuthCoreKit.IAM.Entities.AuditableEntity>())
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

        foreach (var entry in ChangeTracker.Entries<SaaSPlatform.SharedKernel.Common.AuditableEntity>())
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