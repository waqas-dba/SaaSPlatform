using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.SharedKernel.Models
{
    public sealed class ProductOrderInfo
    {
        public Guid Id { get; init; }
        public Guid StoreId { get; init; }
        public string Name { get; init; } = default!;
        public decimal BasePrice { get; init; }
        public bool IsActive { get; init; }
        public bool AvailableForCollection { get; init; }
        public bool AvailableForDelivery { get; init; }
    }
}
