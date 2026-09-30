using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IStoreMenuService
{
    Task<PagedResult<StoreMenuItemDto>> GetMenuAsync(Guid tenantId, Guid storeId, ProductFilterQuery filter, CancellationToken ct = default);

    /// <summary>Makes products available at a store. Returns how many were newly added.</summary>
    Task<int> AssignAsync(Guid tenantId, Guid storeId, AssignProductsRequest request, CancellationToken ct = default);

    Task UpdateAsync(Guid tenantId, Guid storeId, Guid productId, UpdateStoreProductRequest request, CancellationToken ct = default);
    Task UnassignAsync(Guid tenantId, Guid storeId, Guid productId, CancellationToken ct = default);

    /// <summary>Copies the source store's menu into the target store.</summary>
    Task<MenuTransferResultDto> CopyAsync(Guid tenantId, Guid targetStoreId, MenuTransferRequest request, CancellationToken ct = default);

    /// <summary>Copies the menu, then empties the source store's menu.</summary>
    Task<MenuTransferResultDto> MoveAsync(Guid tenantId, Guid targetStoreId, MenuTransferRequest request, CancellationToken ct = default);
}