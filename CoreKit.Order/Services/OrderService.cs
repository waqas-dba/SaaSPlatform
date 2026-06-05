// CoreKit.Order/Services/OrderService.cs
using CoreKit.Order.Entities;
using CoreKit.Order.Interfaces;
using CoreKit.Order.Models;
using CoreKit.Order.Persistence;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CoreKit.Order.Services;

public class OrderService : IOrderService
{
    private readonly OrderDbContext _db;
    private readonly IProductOrderInfoProvider _productInfoProvider;
    private readonly IStoreInfoProvider _storeInfoProvider;

    private static readonly Dictionary<OrderStatus, IReadOnlyList<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Pending] = new[] { OrderStatus.Confirmed, OrderStatus.Cancelled },
        [OrderStatus.Confirmed] = new[] { OrderStatus.Preparing, OrderStatus.Cancelled },
        [OrderStatus.Preparing] = new[] { OrderStatus.ReadyForPickup, OrderStatus.OutForDelivery, OrderStatus.Cancelled },
        [OrderStatus.ReadyForPickup] = new[] { OrderStatus.Delivered, OrderStatus.Cancelled },
        [OrderStatus.OutForDelivery] = new[] { OrderStatus.Delivered, OrderStatus.Cancelled },
        [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
    };

    public OrderService(
        OrderDbContext db,
        IProductOrderInfoProvider productInfoProvider,
        IStoreInfoProvider storeInfoProvider)
    {
        _db = db;
        _productInfoProvider = productInfoProvider;
        _storeInfoProvider = storeInfoProvider;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var orderType = request.OrderType.Equals("Delivery", StringComparison.OrdinalIgnoreCase)
            ? OrderType.Delivery : OrderType.Collection;

        if (orderType == OrderType.Delivery && string.IsNullOrWhiteSpace(request.DeliveryAddress))
            throw new InvalidOperationException("Delivery address is required for delivery orders.");

        var storeInfo = await _storeInfoProvider.GetStoreInfoAsync(request.StoreId, ct)
            ?? throw new KeyNotFoundException("Store not found.");

        var order = new CustomerOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            StoreId = request.StoreId,
            TenantId = storeInfo.TenantId,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            Type = orderType,
            Status = OrderStatus.Pending,
            DeliveryAddress = orderType == OrderType.Delivery ? request.DeliveryAddress : null,
            DeliveryFee = orderType == OrderType.Delivery ? request.DeliveryFee : null,
            Notes = request.Notes
        };

        decimal subTotal = 0;
        foreach (var itemReq in request.Items)
        {
            var productInfo = await _productInfoProvider.GetProductInfoAsync(itemReq.ProductId, ct)
                ?? throw new KeyNotFoundException($"Product {itemReq.ProductId} not found.");

            if (productInfo.StoreId != request.StoreId)
                throw new InvalidOperationException($"Product '{productInfo.Name}' does not belong to this store.");

            if (!productInfo.IsActive)
                throw new InvalidOperationException($"Product '{productInfo.Name}' is not available.");

            if (orderType == OrderType.Collection && !productInfo.AvailableForCollection)
                throw new InvalidOperationException($"Product '{productInfo.Name}' is not available for collection.");
            if (orderType == OrderType.Delivery && !productInfo.AvailableForDelivery)
                throw new InvalidOperationException($"Product '{productInfo.Name}' is not available for delivery.");

            string? variantName = null;
            decimal unitPrice = productInfo.BasePrice;

            if (itemReq.ProductVariantId.HasValue)
            {
                var variantInfo = await _productInfoProvider.GetVariantInfoAsync(itemReq.ProductVariantId.Value, ct)
                    ?? throw new KeyNotFoundException($"Variant {itemReq.ProductVariantId} not found.");
                if (!variantInfo.IsActive)
                    throw new InvalidOperationException("The selected variant is not available.");
                variantName = variantInfo.Sku;
                unitPrice = variantInfo.Price;
            }

            string? addonsJson = null;
            if (itemReq.Addons != null && itemReq.Addons.Any())
                addonsJson = JsonSerializer.Serialize(itemReq.Addons);

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = itemReq.ProductId,
                ProductVariantId = itemReq.ProductVariantId,
                ProductName = productInfo.Name,
                VariantName = variantName,
                Quantity = itemReq.Quantity,
                UnitPrice = unitPrice,
                AddonsJson = addonsJson
            };
            order.Items.Add(orderItem);
            subTotal += orderItem.Quantity * orderItem.UnitPrice;
        }

        order.SubTotal = subTotal;
        order.Total = subTotal + (order.DeliveryFee ?? 0);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        return MapToDto(order);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct);
        return order is null ? null : MapToDto(order);
    }

    public async Task<PagedResult<OrderDto>> GetByStoreAsync(Guid storeId, PagedQuery query, CancellationToken ct = default)
    {
        var baseQuery = _db.Orders.Where(o => o.StoreId == storeId);
        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip(query.Skip).Take(query.PageSize)
            .Select(o => MapToDto(o))
            .ToListAsync(ct);
        return PagedResult<OrderDto>.From(items, total, query.Page, query.PageSize);
    }

    public async Task UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync(new object[] { orderId }, ct)
            ?? throw new KeyNotFoundException("Order not found.");

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            throw new InvalidOperationException($"Invalid status '{request.Status}'.");

        if (!AllowedTransitions[order.Status].Contains(newStatus))
            throw new InvalidOperationException($"Cannot transition from '{order.Status}' to '{newStatus}'.");

        if (newStatus == OrderStatus.ReadyForPickup && order.Type != OrderType.Collection)
            throw new InvalidOperationException("ReadyForPickup status is only valid for collection orders.");
        if (newStatus == OrderStatus.OutForDelivery && order.Type != OrderType.Delivery)
            throw new InvalidOperationException("OutForDelivery status is only valid for delivery orders.");

        order.Status = newStatus;
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync(new object[] { orderId }, ct)
            ?? throw new KeyNotFoundException("Order not found.");
        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel an already completed or cancelled order.");
        order.Status = OrderStatus.Cancelled;
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(ct);
    }

    private static string GenerateOrderNumber()
        => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

    private static OrderDto MapToDto(CustomerOrder o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        CustomerName = o.CustomerName,
        CustomerPhone = o.CustomerPhone,
        Type = o.Type.ToString(),
        Status = o.Status.ToString(),
        DeliveryAddress = o.DeliveryAddress,
        DeliveryFee = o.DeliveryFee,
        SubTotal = o.SubTotal,
        Total = o.Total,
        Notes = o.Notes,
        Items = o.Items.Select(i => new OrderItemDto
        {
            ProductName = i.ProductName,
            VariantName = i.VariantName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            LineTotal = i.Quantity * i.UnitPrice,
            Addons = string.IsNullOrWhiteSpace(i.AddonsJson)
                ? null
                : JsonSerializer.Deserialize<List<AddonSnapshotDto>>(i.AddonsJson)
        }).ToList()
    };
}