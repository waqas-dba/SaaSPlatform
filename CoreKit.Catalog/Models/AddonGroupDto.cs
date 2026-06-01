// AddonGroupDto.cs
namespace CoreKit.Catalog.Models;

public class AddonGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<AddonDto> Addons { get; set; } = new();
}