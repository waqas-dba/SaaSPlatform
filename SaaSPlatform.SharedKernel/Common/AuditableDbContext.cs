// CoreKit.SharedKernel | Common/AuditableDbContext.cs
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Text.Json;

namespace CoreKit.SharedKernel.Common;

public abstract class AuditableDbContext : DbContext
{
    private readonly ICurrentUser? _currentUser;

    protected AuditableDbContext(DbContextOptions options, ICurrentUser? currentUser = null)
        : base(options) { _currentUser = currentUser; }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditRules()
    {
        var utcNow = DateTime.UtcNow;
        var userId = _currentUser?.UserId;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Unchanged) continue;

            if (entry.Entity is AuditableEntity auditable)
            {
                if (entry.State == EntityState.Added) { auditable.CreatedAt = utcNow; auditable.CreatedBy = userId; }
                if (entry.State == EntityState.Modified) { auditable.UpdatedAt = utcNow; auditable.UpdatedBy = userId; }
            }

            if (entry.Entity is ISoftDelete softDelete && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDelete.IsDeleted = true;
                softDelete.DeletedAtUtc = utcNow;
                softDelete.DeletedBy = userId;
            }

            if (entry.Entity is ITenantScoped tenantEntity && entry.State == EntityState.Added && tenantEntity.TenantId == Guid.Empty)
                throw new InvalidOperationException($"TenantId is required for {entry.Entity.GetType().Name}");
        }

        // Audit trail – only if the model actually contains AuditLog
        LogAuditTrail();
    }

    private void LogAuditTrail()
    {
        if (Model.FindEntityType(typeof(AuditLog)) == null) return;

        var userId = _currentUser?.UserId?.ToString();
        var auditLogs = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.Entity is AuditLog) continue;

            // Build entity id from primary key (supports composite keys)
            var pk = entry.Metadata.FindPrimaryKey();
            string entityId;
            if (pk != null)
            {
                var pkValues = pk.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "null");
                entityId = string.Join(", ", pkValues);
            }
            else entityId = "unknown";

            var audit = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entry.Entity.GetType().Name,
                EntityId = entityId,
                Action = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Modified => "Updated",
                    EntityState.Deleted => "Deleted",
                    _ => entry.State.ToString()
                },
                ChangedBy = userId,
                Timestamp = DateTime.UtcNow,
                OldValues = entry.State == EntityState.Modified ? SerializeOldValues(entry) : null,
                NewValues = SerializeNewValues(entry)
            };
            auditLogs.Add(audit);
        }
        if (auditLogs.Any()) Set<AuditLog>().AddRange(auditLogs);
    }

    private static string? SerializeOldValues(EntityEntry entry)
    {
        var oldVals = new Dictionary<string, object?>();
        foreach (var prop in entry.OriginalValues.Properties)
        {
            var original = entry.OriginalValues[prop];
            var current = entry.CurrentValues[prop];
            if (!Equals(original, current)) oldVals[prop.Name] = original;
        }
        return oldVals.Count > 0 ? JsonSerializer.Serialize(oldVals) : null;
    }

    private static string SerializeNewValues(EntityEntry entry)
    {
        var newVals = new Dictionary<string, object?>();
        foreach (var prop in entry.CurrentValues.Properties) newVals[prop.Name] = entry.CurrentValues[prop];
        return JsonSerializer.Serialize(newVals);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (typeof(ISoftDelete).IsAssignableFrom(clrType))
                modelBuilder.Entity(clrType).HasQueryFilter(BuildSoftDeleteFilter(clrType));
            if (clrType.GetProperty(nameof(AuditableEntity.RowVersion)) != null)
                modelBuilder.Entity(clrType).Property<uint>(nameof(AuditableEntity.RowVersion))
                    .IsRowVersion().HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate();
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