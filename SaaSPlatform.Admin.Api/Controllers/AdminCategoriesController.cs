// SaaSPlatform.Admin.Api/Controllers/AdminCategoriesController.cs (or Tenant API)
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize]
public class AdminCategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;

    public AdminCategoriesController(ICategoryService categoryService)
        => _categoryService = categoryService;

    [HttpGet("tenant/{tenantId}")]
    [RequiresPermission("catalog.categories.view")]
    public async Task<IActionResult> GetByTenant(Guid tenantId, [FromQuery] Guid? storeId = null)
    {
        var categories = await _categoryService.GetByTenantAsync(tenantId, storeId);
        return Ok(categories);
    }

    [HttpGet("{id}")]
    [RequiresPermission("catalog.categories.view")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    [RequiresPermission("catalog.categories.create")]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
    {
        var category = await _categoryService.CreateAsync(request);
        return CreatedResponse(category);
    }

    [HttpPut("{id}")]
    [RequiresPermission("catalog.categories.update")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request)
    {
        await _categoryService.UpdateAsync(id, request);
        return UpdatedResponse();
    }

    [HttpDelete("{id}")]
    [RequiresPermission("catalog.categories.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _categoryService.DeleteAsync(id);
        return DeletedResponse();
    }
}