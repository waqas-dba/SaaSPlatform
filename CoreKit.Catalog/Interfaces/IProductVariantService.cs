// CoreKit.Catalog/Interfaces/IProductVariantService.cs
using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IProductVariantService
{
    Task<List<ProductVariantDto>> GetByProductAsync(Guid productId);
    Task<ProductVariantDto?> GetByIdAsync(Guid id);
    Task<ProductVariantDto> CreateAsync(Guid productId, CreateVariantRequest request);
    Task UpdateAsync(Guid id, UpdateVariantRequest request);
    Task DeleteAsync(Guid id);
}