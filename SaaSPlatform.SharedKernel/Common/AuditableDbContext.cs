using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.SharedKernel.Common;

/// <summary>
/// Base DbContext that automatically sets audit fields (CreatedAt, CreatedBy, etc.)
/// and enforces soft‑delete filtering.
/// </summary>
public abstract class AuditableDbContext : DbContext
{
    private readonly ICurrentUser? _currentUser;

    protected AuditableDbContext(
        DbContextOptions options,
        ICurrentUser? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
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

                    // Handle soft‑delete
                    if (entry.Entity is ISoftDelete deletable &&
                        entry.Property(nameof(ISoftDelete.IsDeleted)).IsModified &&
                        deletable.IsDeleted)
                    {
                        deletable.DeletedAtUtc = DateTime.UtcNow;
                        deletable.DeletedBy = _currentUser?.UserId;
                    }
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter for soft‑delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }
    }

    private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
        var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(condition, parameter);
    }
}