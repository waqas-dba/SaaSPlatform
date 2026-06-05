using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.Order.Models;

public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public List<AddonSnapshotDto>? Addons { get; set; }
}
