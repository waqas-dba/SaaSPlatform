using System.Text.Json;
using CoreKit.Order.Entities;
using CoreKit.Order.Interfaces;
using CoreKit.Order.Models;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Models;

namespace CoreKit.Order.Services;

public class OrderService : IOrderService
{
    private const int MaxItemsPerOrder = 50;
    private const int MaxQuantityPerItem = 99;
    private const decimal MaxDeliveryFee = 100_000m;

    private readonly IOrderRepository _orderRepo;
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
        IOrderRepository orderRepo,
        IProductOrderInfoProvider productInfoProvider,
        IStoreInfoProvider storeInfoProvider)
    {
        _orderRepo = orderRepo;
        _productInfoProvider = productInfoProvider;
        _storeInfoProvider = storeInfoProvider;
    }

    public async Task<OrderDto> CreateAsync(
        Guid tenantId, CreateOrderRequest request, CancellationToken ct = default)
    {
        var orderType = ParseOrderType(request.OrderType);

        var customerName = (request.CustomerName ?? string.Empty).Trim();
        var customerPhone = (request.CustomerPhone ?? string.Empty).Trim();

        if (customerName.Length == 0 || customerName.Length > 200)
            throw new ArgumentException("Customer name is required (max 200 characters).");

        if (customerPhone.Length == 0 || customerPhone.Length > 20)
            throw new ArgumentException("Customer phone is required (max 20 characters).");

        var notes = request.Notes?.Trim();
        if (notes is { Length: > 1000 })
            throw new ArgumentException("Notes are too long (max 1000 characters).");

        if (request.Items is null || request.Items.Count == 0)
            throw new ArgumentException("An order must contain at least one item.");

        if (request.Items.Count > MaxItemsPerOrder)
            throw new ArgumentException($"An order can contain at most {MaxItemsPerOrder} items.");

        var store = await _storeInfoProvider.GetStoreInfoAsync(request.StoreId, ct);
        if (store is null || store.TenantId != tenantId)
            throw new KeyNotFoundException("Store not found.");

        string? deliveryAddress = null;
        decimal? deliveryFee = null;
        string? tableNumber = null;
        string? roomNumber = null;

        switch (orderType)
        {
            case OrderType.Delivery:
                deliveryAddress = RequireText(request.DeliveryAddress, "Delivery address is required for delivery orders.", 500);
                deliveryFee = request.DeliveryFee ?? 0m;
                if (deliveryFee < 0 || deliveryFee > MaxDeliveryFee)
                    throw new ArgumentException("Delivery fee is not valid.");
                break;

            case OrderType.DineIn:
                tableNumber = RequireText(request.TableNumber, "Table number is required for dine-in orders.", 20);
                break;

            case OrderType.RoomService:
                roomNumber = RequireText(request.RoomNumber, "Room number is required for room service orders.", 20);
                break;
        }

        var order = new CustomerOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            StoreId = request.StoreId,
            TenantId = tenantId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            Type = orderType,
            Status = OrderStatus.Pending,
            DeliveryAddress = deliveryAddress,
            DeliveryFee = deliveryFee,
            TableNumber = tableNumber,
            RoomNumber = roomNumber,
            Notes = notes
        };

        decimal subTotal = 0;

        foreach (var itemReq in request.Items)
        {
            if (itemReq.Quantity < 1 || itemReq.Quantity > MaxQuantityPerItem)
                throw new ArgumentException($"Quantity must be between 1 and {MaxQuantityPerItem}.");

            var product = await _productInfoProvider.GetProductInfoAsync(itemReq.ProductId, request.StoreId, ct);
            if (product is null || product.TenantId != tenantId)
                throw new InvalidOperationException($"Product {itemReq.ProductId} is not on this store's menu.");

            if (!product.IsActive)
                throw new InvalidOperationException($"'{product.Name}' is not available right now.");

            if (!IsAvailableForType(product, orderType))
                throw new InvalidOperationException($"'{product.Name}' is not available for {Describe(orderType)}.");

            string? variantName = null;
            var unitPrice = product.UnitPrice;

            if (itemReq.ProductVariantId.HasValue)
            {
                var variant = await _productInfoProvider.GetVariantInfoAsync(
                    itemReq.ProductVariantId.Value, product.Id, ct)
                    ?? throw new InvalidOperationException(
                        $"The selected option does not belong to '{product.Name}'.");

                if (!variant.IsActive)
                    throw new InvalidOperationException($"The selected option of '{product.Name}' is not available.");

                variantName = variant.Name;
                unitPrice = variant.Price;
            }
            else if (product.HasVariants)
            {
                throw new InvalidOperationException($"Please choose an option for '{product.Name}'.");
            }

            var selection = await _productInfoProvider.ResolveAddonsAsync(
                product.Id, itemReq.AddonIds ?? new List<Guid>(), ct);

            if (!selection.IsValid)
                throw new InvalidOperationException($"'{product.Name}': {string.Join(" ", selection.Errors)}");

            var addonsTotal = selection.Addons.Sum(a => a.AdditionalPrice);

            var addonsJson = selection.Addons.Count == 0
                ? null
                : JsonSerializer.Serialize(selection.Addons.Select(a => new AddonSnapshotDto
                {
                    Name = a.Name,
                    Price = a.AdditionalPrice
                }));

            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                ProductVariantId = itemReq.ProductVariantId,
                ProductName = product.Name,
                VariantName = variantName,
                Quantity = itemReq.Quantity,
                UnitPrice = unitPrice,
                AddonsTotal = addonsTotal,
                AddonsJson = addonsJson
            });

            subTotal += itemReq.Quantity * (unitPrice + addonsTotal);
        }

        order.SubTotal = subTotal;
        order.Total = subTotal + (order.DeliveryFee ?? 0m);

        _orderRepo.Add(order);
        await _orderRepo.SaveChangesAsync(ct);

        return MapToDto(order);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid tenantId, Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(tenantId, orderId, track: false, ct);
        return order is null ? null : MapToDto(order);
    }

    public async Task<PagedResult<OrderDto>> GetByStoreAsync(
        Guid tenantId, Guid storeId, PagedQuery query, CancellationToken ct = default)
    {
        var page = await _orderRepo.GetByStoreAsync(tenantId, storeId, query, ct);
        var dtos = page.Items.Select(MapToDto).ToList();

        return PagedResult<OrderDto>.From(dtos, page.TotalCount, page.Page, page.PageSize);
    }

    public async Task UpdateStatusAsync(
        Guid tenantId, Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(tenantId, orderId, track: true, ct)
            ?? throw new KeyNotFoundException("Order not found.");

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus) || !Enum.IsDefined(newStatus))
            throw new InvalidOperationException($"Invalid status '{request.Status}'.");

        if (!AllowedTransitions[order.Status].Contains(newStatus))
            throw new InvalidOperationException($"Cannot transition from '{order.Status}' to '{newStatus}'.");

        if (newStatus == OrderStatus.ReadyForPickup &&
            order.Type is not (OrderType.Collection or OrderType.DineIn))
            throw new InvalidOperationException("ReadyForPickup is only valid for collection and dine-in orders.");

        if (newStatus == OrderStatus.OutForDelivery &&
            order.Type is not (OrderType.Delivery or OrderType.RoomService))
            throw new InvalidOperationException("OutForDelivery is only valid for delivery and room service orders.");

        order.Status = newStatus;
        await _orderRepo.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(Guid tenantId, Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(tenantId, orderId, track: true, ct)
            ?? throw new KeyNotFoundException("Order not found.");

        if (!AllowedTransitions[order.Status].Contains(OrderStatus.Cancelled))
            throw new InvalidOperationException("Cannot cancel an order that is already completed or cancelled.");

        order.Status = OrderStatus.Cancelled;
        await _orderRepo.SaveChangesAsync(ct);
    }

    private static OrderType ParseOrderType(string? raw)
    {
        if (!Enum.TryParse<OrderType>(raw?.Trim(), true, out var type) || !Enum.IsDefined(type))
            throw new ArgumentException("OrderType must be Collection, Delivery, DineIn or RoomService.");

        return type;
    }

    private static string RequireText(string? value, string message, int maxLength)
    {
        var text = value?.Trim() ?? string.Empty;
        if (text.Length == 0) throw new ArgumentException(message);
        if (text.Length > maxLength) throw new ArgumentException($"Value is too long (max {maxLength} characters).");
        return text;
    }

    private static bool IsAvailableForType(ProductOrderInfo product, OrderType type) => type switch
    {
        OrderType.Collection => product.AvailableForCollection,
        OrderType.Delivery => product.AvailableForDelivery,
        OrderType.DineIn or OrderType.RoomService => product.AvailableForDineIn,
        _ => false
    };

    private static string Describe(OrderType type) => type switch
    {
        OrderType.Collection => "collection",
        OrderType.Delivery => "delivery",
        OrderType.DineIn => "dine-in",
        OrderType.RoomService => "room service",
        _ => type.ToString()
    };

    private static string GenerateOrderNumber()
        => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";

    private static OrderDto MapToDto(CustomerOrder o) => new()
    {
        Id = o.Id,
        StoreId = o.StoreId,
        OrderNumber = o.OrderNumber,
        CustomerName = o.CustomerName,
        CustomerPhone = o.CustomerPhone,
        Type = o.Type.ToString(),
        Status = o.Status.ToString(),
        DeliveryAddress = o.DeliveryAddress,
        DeliveryFee = o.DeliveryFee,
        TableNumber = o.TableNumber,
        RoomNumber = o.RoomNumber,
        SubTotal = o.SubTotal,
        Total = o.Total,
        Notes = o.Notes,
        Items = o.Items.Select(i => new OrderItemDto
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            VariantName = i.VariantName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            AddonsTotal = i.AddonsTotal,
            LineTotal = i.Quantity * (i.UnitPrice + i.AddonsTotal),
            Addons = string.IsNullOrWhiteSpace(i.AddonsJson)
                ? null
                : JsonSerializer.Deserialize<List<AddonSnapshotDto>>(i.AddonsJson)
        }).ToList()
    };
}