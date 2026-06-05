// SaaSPlatform.SharedKernel/Common/AuditableDbContext.cs
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CoreKit.SharedKernel.Common;

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

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    // FIX: missing overload — EF calls this internally
    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditRules()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Unchanged) continue;

            if (entry.Entity is AuditableEntity auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = utcNow;
                    auditable.CreatedBy = _currentUser?.UserId;
                }

                if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedAt = utcNow;
                    auditable.UpdatedBy = _currentUser?.UserId;
                }
            }

            if (entry.Entity is ISoftDelete softDelete &&
                entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDelete.IsDeleted = true;
                softDelete.DeletedAtUtc = utcNow;
                softDelete.DeletedBy = _currentUser?.UserId;
            }

            if (entry.Entity is ITenantScoped tenantEntity &&
                entry.State == EntityState.Added &&
                tenantEntity.TenantId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    $"TenantId is required for {entry.Entity.GetType().Name}");
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(ISoftDelete).IsAssignableFrom(clrType))
                modelBuilder.Entity(clrType)
                    .HasQueryFilter(BuildSoftDeleteFilter(clrType));

            if (clrType.GetProperty(nameof(AuditableEntity.RowVersion)) != null)
                modelBuilder.Entity(clrType)
                    .Property<uint>(nameof(AuditableEntity.RowVersion))
                    .IsRowVersion()
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate();
        }
    }

    private static LambdaExpression BuildSoftDeleteFilter(Type entityType)
    {
        var param = Expression.Parameter(entityType, "e");
        var prop = Expression.Property(param, nameof(ISoftDelete.IsDeleted));
        var condition = Expression.Equal(prop, Expression.Constant(false));
        return Expression.Lambda(condition, param);
    }
}