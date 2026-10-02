namespace CoreKit.SharedKernel.Models
{
    public sealed class AddonOrderInfo
    {
        public Guid Id { get; init; }
        public Guid GroupId { get; init; }
        public string GroupName { get; init; } = default!;
        public string Name { get; init; } = default!;
        public decimal AdditionalPrice { get; init; }
    }

    public sealed class AddonSelectionResult
    {
        public IReadOnlyList<AddonOrderInfo> Addons { get; init; } = Array.Empty<AddonOrderInfo>();
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
        public bool IsValid => Errors.Count == 0;
    }
}