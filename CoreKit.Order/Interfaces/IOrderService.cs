using CoreKit.Order.Models;
using CoreKit.SharedKernel.Models;

namespace CoreKit.Order.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(Guid tenantId, CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderDto?> GetByIdAsync(Guid tenantId, Guid orderId, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> GetByStoreAsync(Guid tenantId, Guid storeId, PagedQuery query, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid tenantId, Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct = default);
    Task CancelAsync(Guid tenantId, Guid orderId, CancellationToken ct = default);
}