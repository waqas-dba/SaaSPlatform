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

    public async Task<PagedResult<CustomerOrder>> GetByStoreAsync(Guid storeId, PagedQuery query, CancellationToken ct = default)
    {
        var baseQuery = _db.Orders.Where(o => o.StoreId == storeId);
        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(ct);
        return PagedResult<CustomerOrder>.From(items, total, query.Page, query.PageSize);
    }

    public async Task<CustomerOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public void Add(CustomerOrder order) => _db.Orders.Add(order);
    public void Update(CustomerOrder order) => _db.Orders.Update(order);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}