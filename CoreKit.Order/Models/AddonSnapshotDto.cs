using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.Order.Models;

public class AddonSnapshotDto
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}
