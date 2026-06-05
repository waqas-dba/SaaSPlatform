using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.Order.Models;

public class CreateOrderRequest
{
    public Guid StoreId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public string OrderType { get; set; } = default!;
    public string? DeliveryAddress { get; set; }
    public decimal? DeliveryFee { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
    public string? Notes { get; set; }
}
