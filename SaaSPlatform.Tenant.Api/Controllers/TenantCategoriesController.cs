using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
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

    public TenantCategoriesController(
        ICategoryService categoryService,
        ITenantContext tenantContext) : base(tenantContext)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [RequiresPermission("catalog.categories.view")]
    public async Task<IActionResult> GetForTenant(
        [FromQuery] Guid? storeId,
        CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var categories = await _categoryService.GetByTenantAsync(tenantId, storeId, ct);
        return OkResponse(categories);
    }

    [HttpPost]
    [RequiresPermission("catalog.categories.create")]
    public async Task<IActionResult> Create(
        CreateCategoryRequest request,
        CancellationToken ct)
    {
        request.TenantId = RequireTenantId();
        var category = await _categoryService.CreateAsync(request, ct);
        return CreatedResponse(category);
    }

    [HttpPut("{id:guid}")]                                           // NEW
    [RequiresPermission("catalog.categories.update")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken ct)
    {
        await _categoryService.UpdateAsync(id, request, ct);
        return UpdatedResponse();
    }

    [HttpDelete("{id:guid}")]                                        // NEW
    [RequiresPermission("catalog.categories.delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _categoryService.DeleteAsync(id, ct);
        return DeletedResponse();
    }
}