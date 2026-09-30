using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Services;

public sealed class ProductImageService : IProductImageService
{
    private readonly IProductRepository _productRepo;

    public ProductImageService(IProductRepository productRepo) => _productRepo = productRepo;

    public async Task<ProductImageDto> AddAsync(
        Guid tenantId, Guid productId, AddProductImageRequest request, CancellationToken ct = default)
    {
        var product = await LoadAsync(tenantId, productId, ct);

        var url = request.ImageUrl?.Trim();
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("ImageUrl is required.");

        var makePrimary = request.IsPrimary || product.Images.Count == 0;
        if (makePrimary)
            foreach (var existing in product.Images.Where(i => i.IsPrimary))
                existing.IsPrimary = false;

        var image = new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            ImageUrl = url,
            IsPrimary = makePrimary,
            SortOrder = request.SortOrder
        };

        _productRepo.AddImage(image);
        await _productRepo.SaveChangesAsync(ct);

        return CatalogMapper.ToDto(image);
    }

    public async Task RemoveAsync(
        Guid tenantId, Guid productId, Guid imageId, CancellationToken ct = default)
    {
        var product = await LoadAsync(tenantId, productId, ct);
        var image = product.Images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new KeyNotFoundException("Image not found.");

        _productRepo.RemoveImage(image);

        if (image.IsPrimary)
        {
            var next = product.Images
                .Where(i => i.Id != imageId)
                .OrderBy(i => i.SortOrder)
                .FirstOrDefault();

            if (next is not null) next.IsPrimary = true;
        }

        await _productRepo.SaveChangesAsync(ct);
    }

    public async Task ReorderAsync(
        Guid tenantId, Guid productId, ReorderImagesRequest request, CancellationToken ct = default)
    {
        var product = await LoadAsync(tenantId, productId, ct);

        foreach (var item in request.Images)
        {
            var image = product.Images.FirstOrDefault(i => i.Id == item.ImageId)
                ?? throw new KeyNotFoundException($"Image '{item.ImageId}' not found for this product.");

            image.SortOrder = item.SortOrder;
        }

        await _productRepo.SaveChangesAsync(ct);
    }

    public async Task SetPrimaryAsync(
        Guid tenantId, Guid productId, Guid imageId, CancellationToken ct = default)
    {
        var product = await LoadAsync(tenantId, productId, ct);
        var target = product.Images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new KeyNotFoundException("Image not found.");

        foreach (var image in product.Images)
            image.IsPrimary = image.Id == target.Id;

        await _productRepo.SaveChangesAsync(ct);
    }

    private async Task<Product> LoadAsync(Guid tenantId, Guid productId, CancellationToken ct)
        => await _productRepo.GetByIdWithImagesAsync(tenantId, productId, track: true, ct)
           ?? throw new KeyNotFoundException("Product not found.");
}