namespace CoreKit.SharedKernel.Common;

public sealed class TenantScope
{
    public Guid TenantId { get; }

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
        {
            throw new ArgumentException(
                "TenantId cannot be Guid.Empty.",
                nameof(tenantId));
        }

        return new TenantScope(tenantId, false);
    }

    public IQueryable<T> Apply<T>(IQueryable<T> query)
        where T : class, ITenantScoped
    {
        ArgumentNullException.ThrowIfNull(query);

        return IsGlobal
            ? query
            : query.Where(x => x.TenantId == TenantId);
    }
}