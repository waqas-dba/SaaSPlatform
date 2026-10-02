using CoreKit.Order.Entities;
using CoreKit.Order.Interfaces;
using CoreKit.Order.Persistence;
using CoreKit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Order.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _db;

    public OrderRepository(OrderDbContext db) => _db = db;

    public async Task<PagedResult<CustomerOrder>> GetByStoreAsync(
        Guid tenantId, Guid storeId, PagedQuery query, CancellationToken ct = default)
    {
        var baseQuery = _db.Orders
            .AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.StoreId == storeId);

        var total = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return PagedResult<CustomerOrder>.From(items, total, query.Page, query.PageSize);
    }

    public Task<CustomerOrder?> GetByIdAsync(
        Guid tenantId, Guid id, bool track, CancellationToken ct = default)
    {
        IQueryable<CustomerOrder> query = _db.Orders
            .Where(o => o.TenantId == tenantId && o.Id == id)
            .Include(o => o.Items);

        if (!track) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(ct);
    }

    public void Add(CustomerOrder order) => _db.Orders.Add(order);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}