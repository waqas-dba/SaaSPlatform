// CoreKit.Order/Interfaces/IOrderService.cs
using CoreKit.Order.Models;
using CoreKit.SharedKernel.Models;

namespace CoreKit.Order.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> GetByStoreAsync(Guid storeId, PagedQuery query, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct = default);
    Task CancelAsync(Guid orderId, CancellationToken ct = default);
}