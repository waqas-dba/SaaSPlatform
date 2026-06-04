using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin/categories")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class AdminCategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;

    public AdminCategoriesController(ICategoryService categoryService)
        => _categoryService = categoryService;

    [HttpGet("tenant/{tenantId}")]
    [RequiresPermission(Permissions.Catalog.CategoriesView)]
    public async Task<IActionResult> GetByTenant(
        Guid tenantId,
        [FromQuery] Guid? storeId,
        CancellationToken ct)
    {
        var categories = await _categoryService.GetByTenantAsync(tenantId, storeId, ct);
        return OkResponse(categories);
    }

    [HttpGet("{id}")]
    [RequiresPermission(Permissions.Catalog.CategoriesView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);
        return category is null ? NotFound() : OkResponse(category);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Catalog.CategoriesCreate)]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        var category = await _categoryService.CreateAsync(request, ct);
        return CreatedResponse(category);
    }

    [HttpPut("{id}")]
    [RequiresPermission(Permissions.Catalog.CategoriesUpdate)]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken ct)
    {
        await _categoryService.UpdateAsync(id, request, ct);
        return UpdatedResponse();
    }

    [HttpDelete("{id}")]
    [RequiresPermission(Permissions.Catalog.CategoriesDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _categoryService.DeleteAsync(id, ct);
        return DeletedResponse();
    }
}