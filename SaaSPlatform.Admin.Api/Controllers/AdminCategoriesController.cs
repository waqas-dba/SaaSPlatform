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
[Route("api/v{version:apiVersion}/admin/categories/tenant/{tenantId:guid}")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class AdminCategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;

    public AdminCategoriesController(ICategoryService categoryService)
        => _categoryService = categoryService;

    [HttpGet]
    [RequiresPermission(Permissions.Catalog.CategoriesView)]
    public async Task<IActionResult> GetTree(Guid tenantId, CancellationToken ct)
    {
        var categories = await _categoryService.GetTreeAsync(tenantId, ct);
        return OkResponse(categories);
    }

    [HttpGet("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.CategoriesView)]
    public async Task<IActionResult> GetById(Guid tenantId, Guid id, CancellationToken ct)
    {
        var category = await _categoryService.GetByIdAsync(tenantId, id, ct);
        return category is null ? NotFound() : OkResponse(category);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Catalog.CategoriesCreate)]
    public async Task<IActionResult> Create(Guid tenantId, CreateCategoryRequest request, CancellationToken ct)
    {
        var category = await _categoryService.CreateAsync(tenantId, request, ct);
        return CreatedResponse(category);
    }

    [HttpPut("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.CategoriesUpdate)]
    public async Task<IActionResult> Update(
        Guid tenantId, Guid id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var category = await _categoryService.UpdateAsync(tenantId, id, request, ct);
        return OkResponse(category);
    }

    [HttpDelete("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.CategoriesDelete)]
    public async Task<IActionResult> Delete(Guid tenantId, Guid id, CancellationToken ct)
    {
        await _categoryService.DeleteAsync(tenantId, id, ct);
        return DeletedResponse();
    }
}