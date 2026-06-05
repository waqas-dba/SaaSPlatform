using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.SharedKernel.Models
{
    public sealed class ProductVariantOrderInfo
    {
        public Guid Id { get; init; }
        public string Sku { get; init; } = default!;
        public decimal Price { get; init; }
        public bool IsActive { get; init; }
    }

}
