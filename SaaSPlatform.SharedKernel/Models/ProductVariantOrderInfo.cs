namespace CoreKit.SharedKernel.Models
{
    public sealed class ProductVariantOrderInfo
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public string Name { get; init; } = default!;
        public decimal Price { get; init; }
        public bool IsActive { get; init; }
    }
}