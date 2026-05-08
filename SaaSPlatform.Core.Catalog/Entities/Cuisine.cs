// SaaSPlatform.Core/Catalog/Entities/Cuisine.cs
using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class Cuisine : BaseEntity
{
    public string Name { get; set; } = default!;
}