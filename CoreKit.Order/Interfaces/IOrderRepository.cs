// CoreKit.Order/Interfaces/IOrderRepository.cs
using CoreKit.Order.Entities;
using CoreKit.SharedKernel.Models;

namespace CoreKit.Order.Interfaces;

public interface IOrderRepository
{
    Task<PagedResult<CustomerOrder>> GetByStoreAsync(Guid storeId, PagedQuery query, CancellationToken ct = default);
    Task<CustomerOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(CustomerOrder order);
    void Update(CustomerOrder order);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}