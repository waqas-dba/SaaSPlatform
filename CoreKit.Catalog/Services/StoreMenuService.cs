using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Helpers;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Models;

namespace CoreKit.Catalog.Services;

public sealed class StoreMenuService : IStoreMenuService
{
    private readonly IStoreProductRepository _storeProductRepo;
    private readonly IProductRepository _productRepo;
    private readonly IStoreInfoProvider _storeInfoProvider;

    public StoreMenuService(
        IStoreProductRepository storeProductRepo,
        IProductRepository productRepo,
        IStoreInfoProvider storeInfoProvider)
    {
        _storeProductRepo = storeProductRepo;
        _productRepo = productRepo;
        _storeInfoProvider = storeInfoProvider;
    }

    public async Task<PagedResult<StoreMenuItemDto>> GetMenuAsync(
        Guid tenantId, Guid storeId, ProductFilterQuery filter, CancellationToken ct = default)
    {
        await CatalogGuards.RequireStoreAsync(_storeInfoProvider, tenantId, storeId, ct);

        if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice > filter.MaxPrice)
            throw new ArgumentException("MinPrice cannot be greater than MaxPrice.");

        filter.Page = Math.Max(filter.Page, 1);
        filter.PageSize = Math.Clamp(filter.PageSize, 1, 100);

        return await _productRepo.GetStoreMenuAsync(tenantId, storeId, filter, ct);
    }

    public async Task<int> AssignAsync(
        Guid tenantId, Guid storeId, AssignProductsRequest request, CancellationToken ct = default)
    {
        await CatalogGuards.RequireStoreAsync(_storeInfoProvider, tenantId, storeId, ct);

        var productIds = request.ProductIds.Distinct().ToList();
        if (productIds.Count == 0)
            throw new ArgumentException("Select at least one product.");

        var existingProducts = await _productRepo.GetExistingIdsAsync(tenantId, productIds, ct);
        if (existingProducts.Count != productIds.Count)
            throw new KeyNotFoundException("One or more products were not found.");

        var alreadyAssigned = await _storeProductRepo.GetProductIdsAsync(tenantId, storeId, ct);

        var added = 0;
        foreach (var productId in productIds.Where(id => !alreadyAssigned.Contains(id)))
        {
            _storeProductRepo.Add(new StoreProduct
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StoreId = storeId,
                ProductId = productId,
                IsAvailable = true
            });
            added++;
        }

        if (added > 0)
            await _storeProductRepo.SaveChangesAsync(ct);

        return added;
    }

    public async Task UpdateAsync(
        Guid tenantId, Guid storeId, Guid productId, UpdateStoreProductRequest request,
        CancellationToken ct = default)
    {
        await CatalogGuards.RequireStoreAsync(_storeInfoProvider, tenantId, storeId, ct);

        if (request.ClearPriceOverride && request.PriceOverride.HasValue)
            throw new ArgumentException("Send either PriceOverride or ClearPriceOverride, not both.");

        if (request.PriceOverride.HasValue && request.PriceOverride.Value < 0)
            throw new ArgumentException("Price cannot be negative.");

        var storeProduct = await _storeProductRepo.GetAsync(tenantId, storeId, productId, ct)
            ?? throw new KeyNotFoundException("This product is not on the store's menu.");

        if (request.IsAvailable.HasValue) storeProduct.IsAvailable = request.IsAvailable.Value;
        if (request.PriceOverride.HasValue) storeProduct.PriceOverride = request.PriceOverride.Value;
        if (request.ClearPriceOverride) storeProduct.PriceOverride = null;
        if (request.SortOrder.HasValue) storeProduct.SortOrder = request.SortOrder.Value;

        await _storeProductRepo.SaveChangesAsync(ct);
    }

    public async Task UnassignAsync(
        Guid tenantId, Guid storeId, Guid productId, CancellationToken ct = default)
    {
        await CatalogGuards.RequireStoreAsync(_storeInfoProvider, tenantId, storeId, ct);

        var storeProduct = await _storeProductRepo.GetAsync(tenantId, storeId, productId, ct)
            ?? throw new KeyNotFoundException("This product is not on the store's menu.");

        _storeProductRepo.Remove(storeProduct);
        await _storeProductRepo.SaveChangesAsync(ct);
    }

    public Task<MenuTransferResultDto> CopyAsync(
        Guid tenantId, Guid targetStoreId, MenuTransferRequest request, CancellationToken ct = default)
        => TransferAsync(tenantId, targetStoreId, request, move: false, ct);

    public Task<MenuTransferResultDto> MoveAsync(
        Guid tenantId, Guid targetStoreId, MenuTransferRequest request, CancellationToken ct = default)
        => TransferAsync(tenantId, targetStoreId, request, move: true, ct);

    private async Task<MenuTransferResultDto> TransferAsync(
        Guid tenantId, Guid targetStoreId, MenuTransferRequest request, bool move, CancellationToken ct)
    {
        if (request.SourceStoreId == targetStoreId)
            throw new ArgumentException("Source and target store must be different.");

        await CatalogGuards.RequireStoreAsync(_storeInfoProvider, tenantId, request.SourceStoreId, ct);
        await CatalogGuards.RequireStoreAsync(_storeInfoProvider, tenantId, targetStoreId, ct);

        await _storeProductRepo.BeginTransactionAsync(ct);
        try
        {
            await _storeProductRepo.ExecuteAdvisoryLockAsync(LockKeyHelper.GuidToLockKey(targetStoreId), ct);

            var source = await _storeProductRepo.GetByStoreAsync(tenantId, request.SourceStoreId, ct);
            var target = await _storeProductRepo.GetByStoreAsync(tenantId, targetStoreId, ct);

            if (request.ReplaceExisting && target.Count > 0)
            {
                _storeProductRepo.RemoveRange(target);
                await _storeProductRepo.SaveChangesAsync(ct);
                target = new List<StoreProduct>();
            }

            var targetProductIds = target.Select(t => t.ProductId).ToHashSet();
            int copied = 0, skipped = 0;

            foreach (var item in source)
            {
                if (targetProductIds.Contains(item.ProductId))
                {
                    skipped++;
                    continue;
                }

                _storeProductRepo.Add(new StoreProduct
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    StoreId = targetStoreId,
                    ProductId = item.ProductId,
                    IsAvailable = item.IsAvailable,
                    PriceOverride = item.PriceOverride,
                    SortOrder = item.SortOrder
                });
                copied++;
            }

            if (move)
                _storeProductRepo.RemoveRange(source);

            await _storeProductRepo.SaveChangesAsync(ct);
            await _storeProductRepo.CommitAsync(ct);

            return new MenuTransferResultDto { Copied = copied, Skipped = skipped };
        }
        catch
        {
            await _storeProductRepo.RollbackAsync(ct);
            throw;
        }
    }
}