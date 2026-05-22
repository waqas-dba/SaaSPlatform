namespace CoreKit.SharedKernel.Common;

public sealed class StoreScope
{
    public IReadOnlyList<Guid> StoreIds { get; }
    public bool IsAllStores { get; }
    public bool IsEmpty { get; }

    private StoreScope(IReadOnlyList<Guid> storeIds, bool isAllStores, bool isEmpty)
    {
        StoreIds = storeIds;
        IsAllStores = isAllStores;
        IsEmpty = isEmpty;
    }

    public static StoreScope All => new(Array.Empty<Guid>(), true, false);
    public static StoreScope Empty => new(Array.Empty<Guid>(), false, true);

    public static StoreScope For(IEnumerable<Guid> storeIds)
    {
        var ids = storeIds.Distinct().ToList();
        if (ids.Count == 0)
            return Empty;
        return new(ids, false, false);
    }

    public IQueryable<T> Apply<T>(IQueryable<T> query) where T : class, IStoreScoped
    {
        if (IsAllStores) return query;
        if (IsEmpty) return query.Where(e => false); // Return empty result
        return query.Where(e => StoreIds.Contains(e.StoreId));
    }
}