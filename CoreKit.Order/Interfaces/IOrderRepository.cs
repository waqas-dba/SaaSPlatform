using CoreKit.Order.Entities;
using CoreKit.SharedKernel.Models;

namespace CoreKit.Order.Interfaces;

public interface IOrderRepository
{
    Task<PagedResult<CustomerOrder>> GetByStoreAsync(
        Guid tenantId, Guid storeId, PagedQuery query, CancellationToken ct = default);

    Task<CustomerOrder?> GetByIdAsync(
        Guid tenantId, Guid id, bool track, CancellationToken ct = default);

    void Add(CustomerOrder order);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}