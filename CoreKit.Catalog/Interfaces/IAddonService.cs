using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IAddonService
{
    Task<List<AddonDto>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<AddonDto?> GetByIdAsync(Guid addonId, CancellationToken ct = default);
    Task<AddonDto> CreateAdHocAsync(
        Guid productId,
        string name,
        decimal additionalPrice,
        CancellationToken ct = default);
    Task DeleteAsync(Guid addonId, CancellationToken ct = default);
}