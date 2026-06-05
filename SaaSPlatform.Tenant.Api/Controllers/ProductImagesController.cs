// SaaSPlatform.Tenant.Api/Controllers/ProductImagesController.cs
using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/products/{productId:guid}/images")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class ProductImagesController : ApiControllerBase
{
    private readonly IProductImageService _imageService;

    public ProductImagesController(IProductImageService imageService)
        => _imageService = imageService;

    [HttpPost]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> Add(
        Guid productId, AddProductImageRequest request, CancellationToken ct)
    {
        var image = await _imageService.AddAsync(productId, request, ct);
        return CreatedResponse(image);
    }

    [HttpDelete("{imageId:guid}")]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> Remove(
        Guid productId, Guid imageId, CancellationToken ct)
    {
        await _imageService.RemoveAsync(imageId, ct);
        return DeletedResponse();
    }

    [HttpPut("reorder")]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> Reorder(
        Guid productId, ReorderImagesRequest request, CancellationToken ct)
    {
        await _imageService.ReorderAsync(productId, request, ct);
        return UpdatedResponse();
    }

    [HttpPut("{imageId:guid}/primary")]
    [RequiresPermission(Permissions.Catalog.ProductsUpdate)]
    public async Task<IActionResult> SetPrimary(
        Guid productId, Guid imageId, CancellationToken ct)
    {
        await _imageService.SetPrimaryAsync(imageId, ct);
        return UpdatedResponse();
    }
}