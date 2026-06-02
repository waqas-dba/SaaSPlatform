// CoreKit.Catalog/Interfaces/IAddonService.cs
using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IAddonService
{
    Task<List<AddonDto>> GetByProductAsync(Guid productId);
    Task<AddonDto?> GetByIdAsync(Guid addonId);          // new
    Task<AddonDto> CreateAdHocAsync(Guid productId, string name, decimal additionalPrice);
    Task DeleteAsync(Guid addonId);
}