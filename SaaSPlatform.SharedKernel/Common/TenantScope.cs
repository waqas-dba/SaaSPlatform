namespace CoreKit.SharedKernel.Common;

public sealed class TenantScope
{
    public Guid TenantId { get; }       // Guid.Empty when IsGlobal
    public bool IsGlobal { get; }

    private TenantScope(Guid tenantId, bool isGlobal)
    {
        TenantId = tenantId;
        IsGlobal = isGlobal;
    }

    public static TenantScope Global => new(Guid.Empty, true);

    public static TenantScope For(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("tenantId cannot be Guid.Empty.", nameof(tenantId));
        return new(tenantId, false);
    }

    /// <summary>Apply tenant filter to an <see cref="ITenantScoped"/> query.</summary>
    public IQueryable<T> Apply<T>(IQueryable<T> query) where T : class, ITenantScoped
    {
        if (IsGlobal) return query;
        return query.Where(e => e.TenantId == TenantId);
    }
}