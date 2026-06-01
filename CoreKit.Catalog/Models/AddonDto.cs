// AddonDto.cs
namespace CoreKit.Catalog.Models;

public class AddonDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal AdditionalPrice { get; set; }
}