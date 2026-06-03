namespace CoreKit.SharedKernel.Common;

public sealed class StoreScope
{
    public IReadOnlyList<Guid> StoreIds { get; }

    public bool IsAllStores { get; }

    public bool IsEmpty { get; }

    private StoreScope(
        IReadOnlyList<Guid> storeIds,
        bool isAllStores,
        bool isEmpty)
    {
        StoreIds = storeIds;
        IsAllStores = isAllStores;
        IsEmpty = isEmpty;
    }

    public static StoreScope All =>
        new(Array.Empty<Guid>(), true, false);

    public static StoreScope Empty =>
        new(Array.Empty<Guid>(), false, true);

    public static StoreScope For(IEnumerable<Guid> storeIds)
    {
        ArgumentNullException.ThrowIfNull(storeIds);

        var ids = storeIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        return ids.Count == 0
            ? Empty
            : new StoreScope(ids, false, false);
    }

    public IQueryable<T> Apply<T>(IQueryable<T> query)
        where T : class, IStoreScoped
    {
        ArgumentNullException.ThrowIfNull(query);

        if (IsAllStores)
            return query;

        if (IsEmpty)
            return query.Where(_ => false);

        return query.Where(x => StoreIds.Contains(x.StoreId));
    }
}