using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class ProductAddon : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid AddonId { get; set; }
}