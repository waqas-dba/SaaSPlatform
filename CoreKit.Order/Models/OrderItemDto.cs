using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.Order.Models;

public class OrderItemDto
{
    public string ProductName { get; set; } = default!;
    public string? VariantName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public List<AddonSnapshotDto>? Addons { get; set; }
}
