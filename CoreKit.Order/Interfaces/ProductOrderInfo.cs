// CoreKit.Order/Interfaces/IProductOrderInfoProvider.cs
namespace CoreKit.Order.Interfaces;

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

public sealed class ProductVariantOrderInfo
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = default!;
    public decimal Price { get; init; }
    public bool IsActive { get; init; }
}
