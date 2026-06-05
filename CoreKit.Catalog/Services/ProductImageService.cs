// CoreKit.Catalog/Services/ProductImageService.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

public sealed class ProductImageService : IProductImageService
{
    private readonly CatalogDbContext _db;

    public ProductImageService(CatalogDbContext db) => _db = db;

    public async Task<ProductImageDto> AddAsync(
        Guid productId, AddProductImageRequest request,
        CancellationToken ct = default)
    {
        var productExists = await _db.Products
            .AnyAsync(p => p.Id == productId, ct);

        if (!productExists)
            throw new KeyNotFoundException("Product not found.");

        if (request.IsPrimary)
            await ClearPrimaryFlagAsync(productId, ct);

        var image = new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ImageUrl = request.ImageUrl,
            IsPrimary = request.IsPrimary,
            SortOrder = request.SortOrder
        };

        _db.ProductImages.Add(image);
        await _db.SaveChangesAsync(ct);

        return new ProductImageDto
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            IsPrimary = image.IsPrimary
        };
    }

    public async Task RemoveAsync(Guid imageId, CancellationToken ct = default)
    {
        var image = await _db.ProductImages.FindAsync(new object[] { imageId }, ct)
            ?? throw new KeyNotFoundException("Image not found.");

        _db.ProductImages.Remove(image);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReorderAsync(
        Guid productId, ReorderImagesRequest request,
        CancellationToken ct = default)
    {
        var imageIds = request.Images.Select(i => i.ImageId).ToList();

        var images = await _db.ProductImages
            .Where(i => i.ProductId == productId && imageIds.Contains(i.Id))
            .ToListAsync(ct);

        foreach (var item in request.Images)
        {
            var image = images.FirstOrDefault(i => i.Id == item.ImageId);
            if (image is not null)
                image.SortOrder = item.SortOrder;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task SetPrimaryAsync(Guid imageId, CancellationToken ct = default)
    {
        var image = await _db.ProductImages.FindAsync(new object[] { imageId }, ct)
            ?? throw new KeyNotFoundException("Image not found.");

        await ClearPrimaryFlagAsync(image.ProductId, ct);
        image.IsPrimary = true;
        await _db.SaveChangesAsync(ct);
    }

    private async Task ClearPrimaryFlagAsync(Guid productId, CancellationToken ct)
        => await _db.ProductImages
            .Where(i => i.ProductId == productId && i.IsPrimary)
            .ExecuteUpdateAsync(
                s => s.SetProperty(i => i.IsPrimary, false), ct);
}