using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/categories")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class TenantCategoriesController : TenantApiControllerBase
{
    private readonly ICategoryService _categoryService;

    public TenantCategoriesController(ICategoryService categoryService, ITenantContext tenantContext)
        : base(tenantContext)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [RequiresPermission(Permissions.Catalog.CategoriesView)]
    public async Task<IActionResult> GetTree(CancellationToken ct)
    {
        var categories = await _categoryService.GetTreeAsync(RequireTenantId(), ct);
        return OkResponse(categories);
    }

    [HttpGet("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.CategoriesView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var category = await _categoryService.GetByIdAsync(RequireTenantId(), id, ct);
        return category is null ? NotFound() : OkResponse(category);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Catalog.CategoriesCreate)]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        var category = await _categoryService.CreateAsync(RequireTenantId(), request, ct);
        return CreatedResponse(category);
    }

    [HttpPut("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.CategoriesUpdate)]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var category = await _categoryService.UpdateAsync(RequireTenantId(), id, request, ct);
        return OkResponse(category);
    }

    [HttpDelete("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.CategoriesDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _categoryService.DeleteAsync(RequireTenantId(), id, ct);
        return DeletedResponse();
    }
}