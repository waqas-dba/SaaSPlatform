namespace CoreKit.SharedKernel.Common;

public sealed class StoreScope
{
    public IReadOnlyList<Guid> StoreIds { get; }
    public bool IsAllStores { get; }

    private StoreScope(IReadOnlyList<Guid> storeIds, bool isAllStores)
    {
        StoreIds = storeIds;
        IsAllStores = isAllStores;
    }

    public static StoreScope All => new(Array.Empty<Guid>(), true);

    public static StoreScope For(IEnumerable<Guid> storeIds)
    {
        var ids = storeIds.Distinct().ToList();
        if (ids.Count == 0)
            throw new ArgumentException("StoreScope.For requires at least one storeId.");
        return new(ids, false);
    }

    /// <summary>Apply store filter to an <see cref="IStoreScoped"/> query.</summary>
    public IQueryable<T> Apply<T>(IQueryable<T> query) where T : class, IStoreScoped
    {
        if (IsAllStores) return query;
        return query.Where(e => StoreIds.Contains(e.StoreId));
    }
}