namespace CoreKit.SharedKernel.Models
{
    public sealed class ProductOrderInfo
    {
        public Guid Id { get; init; }
        public Guid TenantId { get; init; }
        public Guid StoreId { get; init; }
        public string Name { get; init; } = default!;

        /// <summary>Store price override if set, otherwise the product base price.</summary>
        public decimal UnitPrice { get; init; }

        /// <summary>True only if the product is active, not deleted, assigned to the store and available there.</summary>
        public bool IsActive { get; init; }

        /// <summary>When true the order line must select one of the product's variants.</summary>
        public bool HasVariants { get; init; }

        public bool AvailableForDineIn { get; init; }
        public bool AvailableForCollection { get; init; }
        public bool AvailableForDelivery { get; init; }
    }
}